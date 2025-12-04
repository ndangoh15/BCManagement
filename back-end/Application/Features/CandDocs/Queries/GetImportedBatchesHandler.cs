using Domain.DTO.CandDocs;
using Domain.InterfacesStores.CandDocs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CandDocs.Queries
{
    public class GetImportedBatchesHandler
    {
        private readonly ICandidateRepository _repo;

        public GetImportedBatchesHandler(ICandidateRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ImportedBatchDTO>> HandleAsync()
        {
            return await _repo.GetImportedBatchesAsync();
        }
    }
}
