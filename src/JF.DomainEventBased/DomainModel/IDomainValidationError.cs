using System;
using System.Collections.Generic;
using System.Text;

namespace JF.DomainEventBased.DomainModel
{
   public interface IDomainValidationError
    {
        bool IsValid { get; }

        IEnumerable<DomainValidationErrorItem> GetErrors();

        IDomainValidationError Add(string errorKey);

        IDomainValidationError Add(string errorKey, params object[] parameters);

        IDomainValidationError Add(string errorKey, IList<object> parameters);
    }
}
