using Domain.Entities.CandDocs;
using Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Application.Features.ImportErrors.Commands
{
    public class FixImportErrorCommandHandler
        : IRequestHandler<FixImportErrorCommand, bool>
    {
        private readonly FsContext _context;
        private readonly ILogger<FixImportErrorCommandHandler> _logger;

        public FixImportErrorCommandHandler(FsContext context, ILogger<FixImportErrorCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(FixImportErrorCommand request, CancellationToken cancellationToken)
        {
            using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var document = await _context.CandidateDocuments
                    .Include(d => d.ImportErrors)
                    .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

                if (document == null)
                    throw new Exception("Candidate document not found");

                // 1️⃣ Mise à jour des données corrigées
                document.Session = request.Session;
                document.ExamCode = request.ExamCode;
                document.CentreCode = request.CentreCode;
                document.CandidateNumber = request.CandidateNumber;
                document.CandidateName = request.CandidateName;
                document.IsValid = true;

                // 2️⃣ Suppression des erreurs
                if (document.ImportErrors?.Any() == true)
                {
                    _context.ImportErrors.RemoveRange(document.ImportErrors);
                }

                // 3️⃣ Déplacement du fichier PDF
                MovePdfFromErrorToSuccess(document);

                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to fix import error for document {Id}", request.DocumentId);
                throw;
            }
        }

        private void MovePdfFromErrorToSuccess(CandidateDocument document)
        {
            if (string.IsNullOrWhiteSpace(document.FilePath))
                return;

            var currentPath = document.FilePath;

            if (!System.IO.File.Exists(currentPath))
                return;

            var successPath = currentPath.Replace(
                Path.DirectorySeparatorChar + "errors" + Path.DirectorySeparatorChar,
                Path.DirectorySeparatorChar + "success" + Path.DirectorySeparatorChar
            );

            var successDir = Path.GetDirectoryName(successPath);
            if (!Directory.Exists(successDir))
                Directory.CreateDirectory(successDir);

            System.IO.File.Move(currentPath, successPath, overwrite: true);

            document.FilePath = successPath;
        }
    }
}
