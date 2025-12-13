import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { HttpEvent, HttpEventType } from '@angular/common/http';
import { DocumentManagerService } from 'src/app/services/documents/document-manager.service';
import { firstValueFrom } from 'rxjs';

//const MAX_PARALLEL_UPLOADS = 5;
type ImportStatus = 'waiting' | 'uploading' | 'done' | 'failed' | 'duplicate' | 'invalid';
interface ImportFile {
  file: File;
  decoded: {
    year: string | null;
    examCode: string | null;
    center: string | null;
    batch: string | null;
  };
  progress: number;
  status: ImportStatus;
  blocked: boolean;
}

@Component({
  selector: 'app-import-multiple',
  templateUrl: './import-multiple.component.html',
  styleUrls: ['./import-multiple.component.scss'],
})
export class ImportMultipleComponent {
  @Input() open = false;
  @Output() modalClosed = new EventEmitter<void>();
  @Output() uploaded = new EventEmitter<void>();

  files: ImportFile[] = [];
  uploading = false;

  // stats pour le toast final
  private successCount = 0;
  private failedCount = 0;
  authService: any;

  constructor(
    private docManager: DocumentManagerService,
    private toastr: ToastrService
  ) {}

  private currentUser!: number;

async ngOnInit() {
  this.currentUser = await this.authService.getCurrentUser();
}

  // -------------------------------------------------
  // FERMETURE MODALE
  // -------------------------------------------------
  closeModal() {
    if (this.uploading) return;
    this.open = false;
    this.files = [];
    this.modalClosed.emit();
  }

  // -------------------------------------------------
  // SELECTION FICHIERS + CHECK DUPLICATES
  // -------------------------------------------------
  async onFilesSelected(event: any) {
  this.files = [];
  const selected = Array.from(event.target.files) as File[];
  if (!selected.length) return;

  const names = selected.map(f => f.name);
  let existing: string[] = [];

  try {
    const response = await firstValueFrom(this.docManager.checkAlreadyImported(names));
    existing = response.map((x: any) => x.fileNames); // extract only names
  } catch (e) {
    console.error(e);
    existing = [];
  }

  selected.forEach(file => {
    const decoded = this.decodeFilename(file.name);
    const isDuplicate = existing.some(x => x && x.toLowerCase() === file.name.toLowerCase());

    this.files.push({
      file,
      decoded,
      progress: 0,
      status: isDuplicate ? 'duplicate' : decoded.year ? 'waiting' : 'invalid',
      blocked: isDuplicate || !decoded.year
    });
  });

  
}

  // -------------------------------------------------
  // DECODAGE NOM FICHIER
  // -------------------------------------------------
  decodeFilename(name: string): {
  year: string | null,
  examCode: string | null,
  center: string | null,
  batch: string | null
} {
  const clean = name.replace('.pdf', '');
  const parts = clean.split('_');

  if (parts.length !== 4) {
    return { year: null, examCode: null, center: null, batch: null };
  }

  const [p0, p1, center, batch] = parts;

  let year = '';
  let exam = '';

  if (this.isYearLike(p0)) {
    year = p0;
    exam = p1;
  } else if (this.isYearLike(p1)) {
    exam = p0;
    year = p1;
  } else {
    // pas de modèle valide
    return { year: null, examCode: null, center: null, batch: null };
  }

  return { year, examCode: exam, center, batch };
}

  private isYearLike(value: string): boolean {
    const n = Number(value);
    return !isNaN(n) && n >= 2000 && n <= 2099;
  }


  // -------------------------------------------------
  // UPLOAD PARALLELE LIMITE A 5
  // -------------------------------------------------
  readonly MAX_PARALLEL = 5;

 async uploadAll() {
    const validFiles = this.files.filter(
      f => !f.blocked && f.status === 'waiting'
    );

    if (!validFiles.length) {
      this.toastr.warning('No valid files to upload.');
      return;
    }

    this.uploading = true;
    this.successCount = 0;
    this.failedCount = 0;

    // queue = copie des fichiers valides
    const queue: ImportFile[] = [...validFiles];

    // nombre de workers simultanés (<= 5)
    const workerCount = Math.min(this.MAX_PARALLEL, queue.length);
    const workers: Promise<void>[] = [];

    const worker = async () => {
      while (queue.length > 0) {
        const f = queue.shift();
        if (!f) break;
        await this.uploadOne(f);
      }
    };

    for (let i = 0; i < workerCount; i++) {
      workers.push(worker());
    }

    // attendre que tous les workers finissent
    await Promise.all(workers);

    // fin globale
    this.finishUpload();
  }

async startLimitedParallel(queue: ImportFile[]) {
  const workers = Array(this.MAX_PARALLEL).fill(null).map(() => this.worker(queue));
  await Promise.all(workers);
  this.finishUpload();
}
async worker(queue: ImportFile[]) {
  while (queue.length > 0) {
    const file = queue.shift();
    if (!file) return;

    await this.uploadOne(file);
  }
}

    
private finishUpload() {
    this.uploading = false;

    const failed = this.files.filter(
      f => f.status === 'failed' || f.status === 'invalid'
    );

    if (failed.length === 0) {
      this.toastr.success('All files imported successfully!', 'Completed');
    } else {
      this.toastr.error(
        `${failed.length} file(s) failed. Review the list.`,
        'Error'
      );
    }

    // rafraîchir la liste des documents
    this.docManager.emitRefreshList();

    // ⚠️ ON NE FERME PAS LA MODALE
    // this.closeModal();
  }




  // Upload d’un fichier (promesse)
   private uploadOneOld(f: ImportFile): Promise<void> {
    f.status = 'uploading';
    f.progress = 0;

    return new Promise<void>(async (resolve) => {
      (await this.docManager.uploadSingleFile(f.file, f.decoded,this.currentUser)).subscribe({
        next: (event: HttpEvent<any>) => {
          if (event.type === HttpEventType.UploadProgress) {
            const total = event.total ?? event.loaded;
            f.progress = Math.round((event.loaded / total) * 100);
          }
          if (event.type === HttpEventType.Response) {
            f.status = 'done';
            f.progress = 100;
            this.successCount++;
            resolve();
          }
        },
        error: () => {
          f.status = 'failed';
          this.failedCount++;
          this.toastr.error(`Failed to import ${f.file.name}`, 'Error', {
            timeOut: 3000,
            progressBar: true,
          });
          resolve();
        },
      });
    });
  }

    private uploadOne(f: ImportFile): Promise<void> {
  f.status = 'uploading';
  f.progress = 0;

  return new Promise<void>(async (resolve) => {
    (await this.docManager.uploadSingleFile(f.file, f.decoded,this.currentUser)).subscribe({
      next: (event: HttpEvent<any>) => {

        if (event.type === HttpEventType.UploadProgress) {

          //  PROTECTION ABSOLUE
          if (typeof event.loaded !== 'number') {
            return;
          }

          if (typeof event.total === 'number' && event.total > 0) {
            f.progress = Math.round((event.loaded / event.total) * 100);
          } else {
            // fallback IIS / prod
            f.progress = 0;
          }
        }

        if (event.type === HttpEventType.Response) {
          f.status = 'done';
          f.progress = 100;
          this.successCount++;
          resolve();
        }
      },

      error: (err) => {
        console.error('Upload error:', err);
        f.status = 'failed';
        this.failedCount++;
        this.toastr.error(`Failed to import ${f.file.name}`, 'Error', {
          timeOut: 3000,
          progressBar: true,
        });
        resolve();
      },
    });
  });
}


  // -------------------------------------------------
  // GETTERS POUR L’UI
  // -------------------------------------------------
get duplicateCount(): number {
    return this.files?.filter(f => f.blocked).length ?? 0;
  }

  get validCount(): number {
    return this.files?.filter(
      f => !f.blocked && f.status === 'waiting'
    ).length ?? 0;
  }
  /*get validCount(): number {
    return this.files.filter((f) => !f.blocked).length;
  }*/

  get canUpload(): boolean {
    return this.validCount > 0 && !this.uploading;
  }

  // progression globale = moyenne des progress des fichiers valides
  get overallProgress(): number {
    const valids = this.files.filter((f) => !f.blocked);
    if (!valids.length) return 0;
    const sum = valids.reduce((acc, f) => acc + (f.progress || 0), 0);
    return Math.round(sum / valids.length);
  }
}
/*function resolve() {
  throw new Error('Function not implemented.');
}*/

