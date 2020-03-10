//using Pixel.Automation.Core;
//using Pixel.Automation.Core.Attributes;
//using Pixel.Automation.Core.Exceptions;
//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;

//namespace Pixel.Automation.CoreComponents
//{
//    [DataContract]
//    [Serializable]
//    [ToolBoxItem("With SearchRoot", "Core Components", iconSource: null, description: "Restricts control lookup to descendants of control set as current root", tags: new string[] { "Search Root", "Core" })]
//    public class WithSearchRootActorComponent : Entity
//    {

//        public override IEnumerable<IComponent> GetNextComponentToProcess()
//        {
//            Entity searchRootBlock = this.GetComponentsByName("SearchRoot").Single() as Entity;
//            Entity statements = this.GetComponentsByName("Statements").Single() as Entity;

//            var controlReference = searchRootBlock.GetFirstComponentOfType<ControlReferenceComponent>();
//            var controlIdentity = controlReference?.GetControlDetails() as ControlIdentityComponent;
//            var controlLocator = controlIdentity?.ControlLocator;
//            if (controlLocator == null)
//                throw new MissingComponentException("SearchRoot Block doesn't contain a ControlReference Component");
//            try
//            {
//                controlLocator.GetType().GetMethod("SetSearchRoot").Invoke(controlLocator, new[] { controlIdentity });
//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex, $"Failed to Set Search Root");
//            }

//            try
//            {
//                var statementIterator = statements.GetNextComponentToProcess().GetEnumerator();
//                while (statementIterator.MoveNext())
//                {
//                    yield return statementIterator.Current;
//                }
//            }
//            finally
//            {
//                try
//                {
//                    controlLocator.GetType().GetMethod("PopSearchRoot").Invoke(controlLocator,null);
//                }
//                catch (Exception ex)
//                {
//                    Log.Error(ex, $"Failed to Pop Search Root");
//                }
//            }



//        }

//        public override void ResolveDependencies()
//        {
//            if (this.Components.Count() > 0)
//                return;

//            PlaceHolderEntity searchRootBlock = new PlaceHolderEntity("SearchRoot");
//            PlaceHolderEntity ifBlock = new PlaceHolderEntity("Statements");

//            this.AddComponent(searchRootBlock);
//            this.AddComponent(ifBlock);
//        }
//    }
//}
