using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Processors;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Sequences
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Try/Catch", "Sequences", iconSource: null, description: "Try catch sequence", tags: new string[] {"Try", "Catch", "Finally", "Sequence" })]
    public class TryCatchSequence : EntityProcessor
    {
        private readonly ILogger logger = Log.ForContext<TryCatchSequence>();

        [DataMember(Order = 200)]
        [Display(Name = "Exception", GroupName = "Exception  Handling", Order = 10)]
        [Description("[Optional] Exception encountered during processing of try block")]
        public Argument Exception { get; set; } = new OutArgument<Exception>() { CanChangeType = false, Mode = ArgumentMode.DataBound};

        public TryCatchSequence() : base("Try Catch", "TryCatchSequence")
        {

        }

        public override async Task BeginProcessAsync()
        {
            try
            {
                var tryBlock = this.GetComponentsByName("Try", Enums.SearchScope.Children).FirstOrDefault() as Entity;
                await this.ProcessEntity(tryBlock);

            }
            catch (Exception ex)
            {
                logger.Warning($"An error was encountered while processing Try block. {ex.Message}");
                if(this.Exception.IsConfigured())
                {
                    await this.ArgumentProcessor.SetValueAsync<Exception>(Exception, ex);
                }               
                var catchBlock = this.GetComponentsByName("Catch", Enums.SearchScope.Children).FirstOrDefault() as Entity;
                await this.ProcessEntity(catchBlock);
            }
            finally
            {              
                var finallyBlock = this.GetComponentsByName("Finally", Enums.SearchScope.Children).FirstOrDefault() as Entity;
                await this.ProcessEntity(finallyBlock);
            }
        }

        public override IEnumerable<IComponent> GetNextComponentToProcess()
        {
            yield break;
        }


        public override void ResolveDependencies()
        {
            if (this.Components.Count() > 0)
            {
                return;
            }

            PlaceHolderEntity tryBlock = new PlaceHolderEntity("Try");
            PlaceHolderEntity catchBlock = new PlaceHolderEntity("Catch");
            PlaceHolderEntity finallyBlock = new PlaceHolderEntity("Finally");
            base.AddComponent(tryBlock);
            base.AddComponent(catchBlock);
            base.AddComponent(finallyBlock);
        }

        public override Entity AddComponent(IComponent component)
        {
            return this;
        }
    }
}
