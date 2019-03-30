using System;
using System.Collections.Generic;
using System.Text;

namespace JF.DomainEventBased.DomainModel
{
    public class DomainValidationErrorItem
    {
        public DomainValidationErrorItem()
        {
            Parameters = new List<object>();
        }

        public string ErrorKey { get; set; }

        public List<object> Parameters { get; set; }
    }

    public class DomainValidationError : IDomainValidationError
    {
        private List<DomainValidationErrorItem> errorItems = new List<DomainValidationErrorItem>();

        public bool IsValid => errorItems.Count == 0;

        public IDomainValidationError Add(string errorKey)
        {
            errorItems.Add(new DomainValidationErrorItem { ErrorKey = errorKey });
            return this;
        }

        public IDomainValidationError Add(string errorKey, params object[] parameters)
        {
            errorItems.Add(new DomainValidationErrorItem { ErrorKey = errorKey, Parameters = new List<object>(parameters) });
            return this;
        }

        public IDomainValidationError Add(string errorKey, IList<object> parameters)
        {
            errorItems.Add(new DomainValidationErrorItem { ErrorKey = errorKey, Parameters = new List<object>(parameters) });
            return this;
        }

        public IEnumerable<DomainValidationErrorItem> GetErrors()
        {
            return errorItems;
        }
    }
}
