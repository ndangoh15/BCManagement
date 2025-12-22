import {Component,  Input,  Output,  EventEmitter,  OnChanges,  SimpleChanges} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer,SafeResourceUrl } from '@angular/platform-browser';
import { CandidateDocumentEditDto } from 'src/app/models/candidate-document-edit.model';
import { ImportErrorsManager } from 'src/app/services/documents/import-errors.manager';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-fix-import-error',
  templateUrl: './fix-import-error.component.html',
  styleUrls: ['./fix-import-error.component.scss']
})
export class FixImportErrorComponent implements OnChanges {

  @Input() errorId!: number;

  @Output() closed = new EventEmitter<void>();
  @Output() fixed = new EventEmitter<void>();

  loading = true;
  saving = false;

  document!: CandidateDocumentEditDto;
  form!: FormGroup;
  
  apiBaseUrl = environment.apiUrl

  pdfUrl!: string;
  safePdfUrl: SafeResourceUrl | null = null;

  constructor(
    private fb: FormBuilder,
    private manager: ImportErrorsManager,
    private sanitizer: DomSanitizer
  ) {}

   ngOnInit() {
    console.log(' FixImportErrorComponent INIT, errorId =', this.errorId);

    //  FormGroup DOIT exister immédiatement
    this.form = this.fb.group({
      session: [''],
      examCode: [''],
      centreCode: [''],
      candidateNumber: [''],
      candidateName: ['']
    });
  }

    ngOnChanges() {
  if (this.errorId) {
    this.loadDetail();
  }
}

loadDetail() {
  this.loading = true;

  this.manager
    .getErrorDetail(this.errorId)
    .subscribe({
      next: (res) => {
        this.form.patchValue({
          session: res.session,
          examCode: res.examCode,
          centreCode: res.centreCode,
          candidateNumber: res.candidateNumber,
          candidateName: res.candidateName
        });

        this.pdfUrl = this.apiBaseUrl + res.pdfUrl;
        this.safePdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.pdfUrl);
        this.loading = false;
      }
    });
}


  submit(): void {
    if (this.form.invalid || this.saving) return;

    this.saving = true;

    const payload = {
      documentId: this.document.id, //  IMPORTANT
      session: this.form.value.session,
      examCode: this.form.value.examCode,
      centreCode: this.form.value.centreCode,
      candidateNumber: this.form.value.candidateNumber,
      candidateName: this.form.value.candidateName
    };

    this.manager.fixImportError(payload).subscribe({
      next: () => {
        this.saving = false;
        this.fixed.emit();
        this.close();
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  close(): void {
    this.closed.emit();
  }

  

}
