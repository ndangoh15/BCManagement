using Domain.DTO.Requests;
using FluentValidation;

namespace Application.Features.CandDocs.Queries
{

    public class SearchCandidateDocumentsRequestValidator
    : AbstractValidator<SearchCandidateDocumentsRequest>
    {
        public SearchCandidateDocumentsRequestValidator()
        {
            // 🔴 SEUL champ obligatoire
            RuleFor(x => x.CandidateName)
                .NotEmpty()
                .MinimumLength(2)
                .WithMessage("Candidate name must contain at least 2 characters");

            // 🟢 Optionnels
            RuleFor(x => x.Session)
                .GreaterThan(0)
                .When(x => x.Session.HasValue);

            RuleFor(x => x.Page)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 200);
        }
    }
}
