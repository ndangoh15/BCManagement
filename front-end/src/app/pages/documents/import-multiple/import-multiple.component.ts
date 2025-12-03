import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DocumentService } from 'src/app/generated';
import { firstValueFrom } from 'rxjs';
import { closeModal } from 'src/app/helper/helper-function';

@Component({
  selector: 'app-import-multiple',
  templateUrl: './import-multiple.component.html',
  styleUrls: ['./import-multiple.component.scss']
})
export class ImportMultipleComponent {

  importForm!: FormGroup;
  submitted = false;

  selectedFiles: File[] = [];

  constructor(
    private fb: FormBuilder,
    private docService: DocumentService
  ) {

    this.importForm = this.fb.group({
      examYear: [null, Validators.required],
      examCode: [null, Validators.required],
      centerNumber: [null, Validators.required]
    });
  }

  get f() {
    return this.importForm.controls;
  }

  onFilesSelected(event: any) {
    this.selectedFiles = Array.from(event.target.files);
  }

  async uploadFiles() {
  this.submitted = true;

  if (this.importForm.invalid || this.selectedFiles.length === 0) return;

  const year = this.f['examYear'].value;
  const code = this.f['examCode'].value;
  const center = this.f['centerNumber'].value;
  
  // Inject your current logged user
  const userId = 1; // TODO: Replace with real auth service

  try {
    const res = await this.docService.documentControllerUploadMultiple(
      this.selectedFiles,
      year,
      code,
      center,
      userId
    ).toPromise();

    alert('Upload completed!');
    this.closePopup();

  } catch (err) {
    console.error(err);
    alert('Upload failed!');
  }
}


  closePopup() {
    closeModal('import-multiple-modal');
  }
}
