using FluentValidation;
using Project.Base.Domain.Object.Shared;
using Project.Base.Enumerators;

namespace Project.Base.Domain.Validators
{
    public abstract class BaseAbstractValidator<TObject> : AbstractValidator<TObject> where TObject : BaseObjectWithId, new()
    {
        public abstract void AsignInsertValidations();

        public virtual void AsignUpdateValidations()
        {
            AsignObrigatoryId();
        }

        public virtual void AsignDeleteValidations()
        {
            AsignObrigatoryId();
        }

        private void AsignObrigatoryId()
        {
            _ = RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(Globalization.OBRIGATORY_FIELD, "ID"));
        }
    }
}
