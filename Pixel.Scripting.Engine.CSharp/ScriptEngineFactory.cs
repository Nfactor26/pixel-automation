using Pixel.Automation.Core;

namespace Pixel.Scripting.Engine.CSharp
{
    public class ScriptEngineFactory : IScriptEngineFactory
    {
        public IScriptEngine CreateScriptEngine(bool useCaching)
        {
            ScriptExecutor scriptExecutor = default;
            if(useCaching)
            {
                scriptExecutor =  new ScriptCompiler();
            }
            else
            {
                scriptExecutor = new ScriptRunner();
            }

            IScriptEngine scriptEngine = new ScriptEngine(scriptExecutor);
            return scriptEngine;
        }

        public IScriptEngine CreateInteractiveScriptEngine()
        {
            return CreateScriptEngine(false);
        }
    }
}
