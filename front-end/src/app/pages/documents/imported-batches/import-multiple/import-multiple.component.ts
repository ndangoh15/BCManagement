import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { HttpEvent, HttpEventType } from '@angular/common/http';
import { DocumentManagerService } from 'src/app/services/documents/document-manager.service';
import { firstValueFrom } from 'rxjs';


@Component({
  selector: 'app-import-multiple',
  templateUrl: './import-multiple.component.html',
  styleUrls: ['./import-multiple.component.scss'],
})
export class ImportMultipleComponent {
  @Input() open = false;
  @Output() modalClosed = new EventEmitter();
  @Output() uploaded = new EventEmitter();

  files: any[] = [];
  confirmOpen = false;
  uploading = false;

  constructor(private docManager: DocumentManagerService,
    private toastr: ToastrService
  ) {}


  closeModal() {
    if (this.uploading) return;
    this.open = false;
    this.files = [];
    this.modalClosed.emit();
  }

  async onFilesSelected(event: any) {
  this.files = [];
  const selected = Array.from(event.target.files) as File[];
  if (!selected.length) return;

  const names = selected.map(f => f.name);

  let existing: string[] = [];
  try {
    existing = await firstValueFrom(this.docManager.checkAlreadyImported(names));
    console.log('Existing from backend = ', existing);
  } catch (e) {
    console.error('Error while checking existing files', e);
    // en cas d’erreur on considère tout comme non duplicate
    existing = [];
  }

  selected.forEach(file => {
    const decoded = this.decodeFilename(file.name);
    const isDuplicate = existing
      .some(x => x.toLowerCase() === file.name.toLowerCase());

    this.files.push({
      file,
      decoded,
      progress: 0,
      status: isDuplicate ? 'duplicate' : 'waiting',
      blocked: isDuplicate
    });
  });
}


  decodeFilename(name: string) {
  const clean = name.replace('.pdf', '');
  const parts = clean.split('_');

  let year = '';
  let exam = '';
  let center = '';
  let batch = '';

  if (parts.length === 4) {
    const p0 = parts[0];
    const p1 = parts[1];

    if (this.isYearLike(p0) && !this.isYearLike(p1)) {
      // 2025_5100_13028_003
      year = p0;
      exam = p1;
    } else if (!this.isYearLike(p0) && this.isYearLike(p1)) {
      // 5100_2025_11291_001
      exam = p0;
      year = p1;
    } else if (this.isYearLike(p0) && this.isYearLike(p1)) {
      // les deux ressemblent à une année → on prend le 1er comme année
      year = p0;
      exam = p1;
    } else {
      // aucun ne ressemble à une année → on garde ton ancien comportement
      exam = p0;
      year = p1;
    }

    center = parts[2];
    batch  = parts[3];
  }

  return { year, examCode: exam, center, batch };
}

private isYearLike(value: string): boolean {
  const n = Number(value);
  return !isNaN(n) && n >= 2000 && n <= 2099;
}

    uploadAll() {
  if (!this.files.length) return;
  
  this.toastr.info('Import started, please wait...', 'Processing', {
    timeOut: 2000,
    progressBar: true
  });

  this.confirmUpload();
}

  async confirmUpload() {
    this.confirmOpen = false;
    this.uploading = true;
    await this.startSequentialUpload();
  }

  async startSequentialUpload() {
  for (const f of this.files) {

    if (f.blocked) {
      f.status = 'duplicate';
      f.progress = 0;
      continue;  // 🚫 skip doublon
    }

    f.status = 'uploading';
    f.progress = 0;

    await new Promise<void>((resolve) => {
      this.docManager.uploadSingleFile(f.file, f.decoded)
        .subscribe({
          next: (event: HttpEvent<any>) => {
            if (event.type === HttpEventType.UploadProgress) {
              const total = event.total ?? event.loaded;
              f.progress = Math.round(100 * event.loaded / total);
            }
            if (event.type === HttpEventType.Response) {
              f.status = 'done';
              f.progress = 100;
              resolve();
            }
          },
          error: () => {
            f.status = 'failed';

            this.toastr.error(`Failed to import ${f.file.name}`, 'Error', {
              timeOut: 3000,
              progressBar: true
            });

            resolve();
          }
        });
    });
  }

  /*this.toastr.success('All files have been imported successfully!', 'Completed', {
  timeOut: 3500,
  progressBar: true});*/

  this.uploading = false;
  this.uploaded.emit();
  this.docManager.emitRefreshList(); // 🔥 notifier UI que données changent
  setTimeout(() => this.closeModal(), 800);


}

get duplicateCount(): number {
  return this.files?.filter(f => f.blocked).length ?? 0;
}

get validCount(): number {
  return this.files?.filter(f => !f.blocked).length ?? 0;
}

get canUpload(): boolean {
  return this.validCount > 0 && !this.uploading;
}


}
