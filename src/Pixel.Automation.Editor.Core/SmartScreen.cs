﻿using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Editor.Core
{
    public abstract class SmartScreen : Screen, INotifyDataErrorInfo
    {
        #region INotifyDataErrorInfo

        protected Dictionary<string, List<string>> propertyErrors = new Dictionary<string, List<string>>();
        
        public bool HasErrors => propertyErrors.Count() > 0;

        public virtual bool ShowModelErrors => HasErrors && propertyErrors.ContainsKey("") && propertyErrors[""].Count() > 0;

        public virtual void DismissModelErrors()
        {
            ClearErrors("");
            NotifyOfPropertyChange(() => HasErrors);
            NotifyOfPropertyChange(() => ShowModelErrors);
        }

        public IEnumerable GetErrors(string propertyName)
        {           
            if (propertyErrors.ContainsKey(propertyName ?? string.Empty))
            {
                return propertyErrors[propertyName ?? string.Empty];
            }
            return default(IEnumerable);
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { };
      
        protected virtual void AddOrAppendErrors(string propertyName, params string[] errors)
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
        }


        protected virtual void AddOrAppendErrors(string propertyName, IEnumerable<string> errors)
        {
            if (!propertyErrors.ContainsKey(propertyName))
            {
                propertyErrors.Add(propertyName, new List<string>());
            }
            var existingErrors = propertyErrors[propertyName];         
            existingErrors.AddRange(errors);
            OnErrorsChanged(propertyName);           
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
            NotifyOfPropertyChange(() => HasErrors);
            NotifyOfPropertyChange(() => ShowModelErrors);
        }

        #endregion INotifyDataErrorInfo

        protected virtual void ValidateRequiredProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                AddOrAppendErrors(propertyName, "Field is required");
            }
        }

        protected virtual void ValidatePattern(string regExPattern, string propertyName, string propertyValue, string errorMessage)
        {
            Regex regex = new Regex(regExPattern);
            if(!regex.IsMatch(propertyValue))
            {
                AddOrAppendErrors(propertyName, errorMessage);
            }
        }

    }
}
