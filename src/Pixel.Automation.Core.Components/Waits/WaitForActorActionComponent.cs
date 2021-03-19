using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace Pixel.Automation.Core.Components.Waits
{

    //[DataContract]
    //[Serializable]
    //[ToolBoxItem("Wait for Component", "Core Components", iconSource: null, description: "Wait for a specified timeout for an actor component to finish its task", tags: new string[] { "Wait", "Core" })]
    //public class WaitForComponentActionComponent : ActorComponent // , IHandle<ActorProcessedEventArgs>
    //{
    //    double timeOut;
    //    [DataMember]
    //    [Description("Max amount of time in seconds to wait for a specific actor to finish its task.")]
    //    public double TimeOut
    //    {
    //        get
    //        {
    //            return timeOut;
    //        }
    //        set
    //        {
    //            timeOut = value;
    //        }
    //    }

    //    int targetActorId;
    //    [DataMember]
    //    [Description("Id of target actor for which this component will wait until configured timeout period")]
    //    public int TargetActorId
    //    {
    //        get
    //        {
    //            return targetActorId;
    //        }
    //        set
    //        {
    //           targetActorId = value;
    //        }
    //    }

    //    bool errorOnTimeOutOrFailure=true;
    //    [DataMember]
    //    [Description("Indicates whether this component errors if 'timeout / target actor fails' or allows further processing")]
    //    public bool ErrorOnTimeOutOrFailure
    //    {
    //        get
    //        {
    //            return errorOnTimeOutOrFailure;
    //        }
    //        set
    //        {
    //            errorOnTimeOutOrFailure = value;
    //        }
    //    }

    //    [NonSerialized]
    //    ManualResetEvent resetEvent = new ManualResetEvent(false);

    //    [NonSerialized]
    //    bool isSignaled = false;

    //    public WaitForComponentActionComponent() : base("WaitForComponent", "WaitForActionComponent")
    //    {
           
    //    }    

    //    public override void Act()
    //    {
    //        try
    //        {
    //            Log.Information("Waiting for Component with Id : {TargetActorId} to be processed", TargetActorId);           
    //            resetEvent.WaitOne((int)(timeOut*100));
    //            Log.Information("Finished Waiting for Component with Id : {TargetActorId} to be processed", TargetActorId);
    //            if (isSignaled)
    //                return;

    //            if(errorOnTimeOutOrFailure)
    //                throw new TimeoutException($"Actor component with id : {targetActorId} failed to execute within timeOut period of {timeOut} milliseconds");             
    //        }          
    //        finally
    //        {
    //            resetEvent.Reset();
    //        }
    //    }

    //    public void Handle(ActorProcessedEventArgs message)
    //    {          
    //        if(message.ProcessedActor.Id.Equals(this.targetActorId))
    //        {
    //            Log.Debug("Actor processed message received for component : {Id}", message.ProcessedActor.Id);
    //            if (message.IsSuccess)
    //            {
    //                isSignaled = true;
    //                resetEvent.Set();
    //                return;
    //            }
                   
    //            if(errorOnTimeOutOrFailure)
    //            {
    //                isSignaled = false;
    //                resetEvent.Set();
    //                return;
    //            }

    //        }    
    //    }

    //    public override void ResetComponent()
    //    {
    //        EventAggregator.Subscribe(this);
    //        this.isSignaled = false;
    //        resetEvent.Reset();
    //    }

    //    public override void Dispose()
    //    {
    //        base.Dispose();
    //        if(resetEvent!=null)
    //            resetEvent.Dispose();
    //    }


    //    [OnDeserialized]
    //    public new void Initialize(StreamingContext context)
    //    {           
    //        if (this.resetEvent == null)
    //            this.resetEvent = new ManualResetEvent(false);
    //    }

    //}
}
