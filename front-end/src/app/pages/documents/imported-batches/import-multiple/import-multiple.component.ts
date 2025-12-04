import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DocumentService } from 'src/app/generated';
import { HttpEventType } from '@angular/common/http';

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

  constructor(private docService: DocumentService) {}

  closeModal() {
    if (this.uploading) return;
    this.open = false;
    this.files = [];
    this.modalClosed.emit();
  }

  onFilesSelected(event: any) {
    this.files = [];
    const selected = Array.from(event.target.files) as File[];

    selected.forEach((file: File) => {
      const decoded = this.decodeFilename(file.name);

      this.files.push({
        file,
        decoded,
        progress: 0,
        status: 'waiting'
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
      if (parts[0].length === 4) {
        year = parts[0];
        exam = parts[1];
      } else {
        exam = parts[0];
        year = parts[1];
      }
      center = parts[2];
      batch = parts[3];
    }

    return { year, examCode: exam, center, batch };
  }

  uploadAll() {
    if (!this.files.length) return;
    this.confirmOpen = true;
  }

  async confirmUpload() {
    this.confirmOpen = false;
    this.uploading = true;
    await this.startSequentialUpload();
  }

  async startSequentialUpload() {
  for (const f of this.files) {

    f.status = 'uploading';
    f.progress = 0;

    await new Promise<void>((resolve) => {

      this.docService.documentControllerUploadMultiple(
        [f.file],
        f.decoded.year,
        f.decoded.examCode,
        f.decoded.center
      ).subscribe({
        next: event => {
          if (event.type === HttpEventType.UploadProgress) {
            const total = event.total ?? event.loaded; // sécurité
            f.progress = Math.round(100 * event.loaded / total);
          }

          if (event.type === HttpEventType.Response) {
            f.status = 'done';
            resolve();
          }
        },
        error: () => {
          f.status = 'failed';
          resolve();
        }
      });
    });

  }

  // 👇 Tout est terminé
  this.uploading = false;
  this.uploaded.emit();
  setTimeout(() => this.closeModal(), 800); // petit délai pour que l’utilisateur voie 100%
}

}
