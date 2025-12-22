using Domain.DTO.ImportErrors;
using MediatR;

namespace Application.Features.CandDocs.Queries
{
    public class GetImportErrorDetailQuery    : IRequest<CandidateDocumentEditDto>
    {
        public int ErrorId { get; }

        public GetImportErrorDetailQuery(int errorId)
        {
            ErrorId = errorId;
        }
    }

}
