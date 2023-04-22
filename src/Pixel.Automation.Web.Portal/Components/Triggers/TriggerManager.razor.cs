using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Pixel.Persistence.Core.Models;

namespace Pixel.Automation.Web.Portal.Components.Triggers
{
    public partial class TriggerManager : ComponentBase
    {
        [Inject]
        public ISnackbar SnackBar { get; set; }

        [Parameter]
        public IEnumerable<SessionTrigger> Triggers { get; set; }

        [Parameter]
        public EventCallback OnAddItem { get; set; }

        [Parameter]
        public EventCallback<SessionTrigger> OnDeleteItem { get; set; }


        [Parameter]
        public Func<SessionTrigger, SessionTrigger, Task<bool>> OnUpdateItem { get; set; }

        private MudTable<SessionTrigger> table;


        private SessionTrigger selectedTrigger = null;
        private SessionTrigger elementBeforeEdit;
        private string searchString = "";

        void EditItem(SessionTrigger model)
        {
            selectedTrigger = model;
            table.SetEditingItem(model);
            BackupItem(model);
        }

        async Task UpdateItemAsync()
        {
            var success = await this.OnUpdateItem(elementBeforeEdit, selectedTrigger);
            if (!success)
            {
                ResetItemToOriginalValues(selectedTrigger);
            }
        }

        void BackupItem(object element)
        {
            if (element is ICloneable beforeEdit)
            {
                elementBeforeEdit = beforeEdit.Clone() as SessionTrigger;
            }

        }

        void ResetItemToOriginalValues(object element)
        {
            if(element is CronSessionTrigger sessionTrigger && elementBeforeEdit is CronSessionTrigger ebe)
            {
                sessionTrigger.Handler = ebe.Handler;
                sessionTrigger.IsEnabled = ebe.IsEnabled;
                sessionTrigger.CronExpression = ebe.CronExpression;
            }           
        }

        private bool FilterFunc(SessionTrigger element)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;            
            if (element.Handler.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
    }
}
