using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Pixel.Automation.Editor.Core
{
    public abstract class SmartScreen : Screen, INotifyDataErrorInfo
    {
        #region INotifyDataErrorInfo

        protected Dictionary<string, List<string>> propertyErrors = new Dictionary<string, List<string>>();
        
        public bool HasErrors
        {
            get
            {
                return propertyErrors.Count() > 0;
            }
        }
    
        public bool ShowModelErrors
        {
            get => HasErrors && propertyErrors.ContainsKey("") && propertyErrors[""].Count() > 0;           
        }

        public void DismissModelErrors()
        {
            propertyErrors.Remove("");
            NotifyOfPropertyChange(() => ShowModelErrors);
        }

        public IEnumerable GetErrors(string propertyName)
        {           
            if (propertyErrors.ContainsKey(propertyName ?? string.Empty))
                return propertyErrors[propertyName ?? string.Empty];
            return default(IEnumerable);
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { };
      
        protected virtual void AddOrAppendErrors(string propertyName,params string[] errors)
        {
            if (!propertyErrors.ContainsKey(propertyName))
            {
                propertyErrors.Add(propertyName, new List<string>());               
            }
            var existingErrors = propertyErrors[propertyName];
            foreach (var error in errors)
            {
                existingErrors.Add(error);               
            }
            OnErrorsChanged(propertyName);
            NotifyOfPropertyChange(() => ShowModelErrors);
        }


        protected virtual void ClearErrors(string propertyName)
        {
            if (propertyErrors.ContainsKey(propertyName))
            {
                propertyErrors.Remove(propertyName);
            }         
            OnErrorsChanged(propertyName);
        }

        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion INotifyDataErrorInfo

        protected virtual void ValidateRequiredProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                AddOrAppendErrors(propertyName, "Field is required");
            }
        }

    }
}
