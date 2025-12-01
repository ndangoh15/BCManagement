using Domain.Entities.CandDocs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterfacesStores.CandDocs
{
    public interface ICandidateRepository
    {
        Task AddCandidateDocumentAsync(CandidateDocument document);
        Task UpdateAsync(CandidateDocument document);

        Task<CandidateDocument?> GetByIdAsync(int id);
        Task<CandidateDocument?> GetDocumentByIdAsync(int id);

        Task<List<CandidateDocument>> GetDocumentsAsync(string session, string examCode, string centre);

        Task<List<CandidateDocument>> GetInvalidDocumentsAsync(string session, string examCode, string centre);
        Task<List<CandidateDocument>> GetValidDocumentsAsync(string session, string examCode, string centre);

        Task DeleteAsync(int id);
    }
}
