using System;
using Domain.Entities.CandDocs;
using Domain.InterfacesServices.CandDocs;
using Domain.InterfacesStores.CandDocs;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.CandDocs
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly FsContext _ctx;

        public CandidateRepository(FsContext ctx)
        {
            _ctx = ctx;
        }

        // ----------------------------------------
        // INSERT NEW DOCUMENT
        // ----------------------------------------
        public async Task AddCandidateDocumentAsync(CandidateDocument document)
        {
            await _ctx.CandidateDocuments.AddAsync(document);
            await _ctx.SaveChangesAsync();
        }

        // ----------------------------------------
        // UPDATE DOCUMENT (After Correction)
        // ----------------------------------------
        public async Task UpdateAsync(CandidateDocument document)
        {
            _ctx.CandidateDocuments.Update(document);
            await _ctx.SaveChangesAsync();
        }

        // ----------------------------------------
        // GET DOCUMENT BY ID
        // ----------------------------------------
        public async Task<CandidateDocument?> GetByIdAsync(int id)
        {
            return await _ctx.CandidateDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ----------------------------------------
        // LIST ALL DOCUMENTS FOR THE CENTRE+SESSION
        // ----------------------------------------
        public async Task<List<CandidateDocument>> GetDocumentsAsync(
            string session, string examCode, string centre)
        {
            return await _ctx.CandidateDocuments
                .Where(x => x.Session.ToString() == session &&
                            x.CentreCode == centre &&
                            x.FormCentreCode == centre)
                .OrderBy(x => x.CandidateNumber)
                .ToListAsync();
        }

        // ----------------------------------------
        // LIST ONLY INVALID DOCUMENTS
        // ----------------------------------------
        public async Task<List<CandidateDocument>> GetInvalidDocumentsAsync(
            string session, string examCode, string centre)
        {
            return await _ctx.CandidateDocuments
                .Where(x => x.Session.ToString() == session &&
                            x.CentreCode == centre &&
                            x.IsValid == false)
                .OrderBy(x => x.CandidateNumber)
                .ToListAsync();
        }

        // ----------------------------------------
        // LIST ONLY VALID DOCUMENTS
        // ----------------------------------------
        public async Task<List<CandidateDocument>> GetValidDocumentsAsync(
            string session, string examCode, string centre)
        {
            return await _ctx.CandidateDocuments
                .Where(x => x.Session.ToString() == session &&
                            x.CentreCode == centre &&
                            x.IsValid == true)
                .OrderBy(x => x.CandidateNumber)
                .ToListAsync();
        }

        // ----------------------------------------
        // DELETE
        // ----------------------------------------
        public async Task DeleteAsync(int id)
        {
            var entity = await _ctx.CandidateDocuments.FindAsync(id);
            if (entity != null)
            {
                _ctx.CandidateDocuments.Remove(entity);
                await _ctx.SaveChangesAsync();
            }
        }
        // ---------------------------
        // GET BY ID (new recommended)
        // ---------------------------
        public async Task<CandidateDocument?> GetDocumentByIdAsync(int id)
        {
            return await _ctx.CandidateDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
