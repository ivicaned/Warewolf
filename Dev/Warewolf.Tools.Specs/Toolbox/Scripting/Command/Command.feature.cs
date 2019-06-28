﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.2.0
//      SpecFlow Generator Version:2.3.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Warewolf.Tools.Specs.Toolbox.Scripting.Command
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class CommandFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "Command.feature"
#line hidden
        
        public virtual Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestContext
        {
            get
            {
                return this._testContext;
            }
            set
            {
                this._testContext = value;
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Command", "\tIn order to execute command line scripts\r\n\tAs a Warewolf user\r\n\tI want a tool th" +
                    "at allows me to execute commands ", ProgrammingLanguage.CSharp, new string[] {
                        "Scripting"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public virtual void TestInitialize()
        {
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Title != "Command")))
            {
                global::Warewolf.Tools.Specs.Toolbox.Scripting.Command.CommandFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>(TestContext);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute commands")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteCommands()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute commands", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I have a command variable \"[[drive]]\" equal to \"C:\\\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2397 = new TechTalk.SpecFlow.Table(new string[] {
                        "script"});
            table2397.AddRow(new string[] {
                        "@echo off"});
            table2397.AddRow(new string[] {
                        "REM Testing multiple commands"});
            table2397.AddRow(new string[] {
                        "dir [[drive]]"});
#line 9
 testRunner.Given("I have these command scripts to execute in a single execution run", ((string)(null)), table2397, "Given ");
#line 14
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
 testRunner.Then("the result of the command tool will be \"Volume in drive C has no label\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 16
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2398 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table2398.AddRow(new string[] {
                        "String = String"});
#line 17
 testRunner.And("the debug inputs as", ((string)(null)), table2398, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2399 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2399.AddRow(new string[] {
                        "[[result]] = String"});
#line 20
 testRunner.And("the debug output as", ((string)(null)), table2399, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a command that requires user interaction like pause")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteACommandThatRequiresUserInteractionLikePause()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a command that requires user interaction like pause", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
 testRunner.Given("I have this command script to execute \"pause\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.Then("the result of the command tool will be \"Press any key to continue . . .\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 28
 testRunner.And("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2400 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table2400.AddRow(new string[] {
                        "pause"});
#line 29
 testRunner.And("the debug inputs as", ((string)(null)), table2400, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2401 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2401.AddRow(new string[] {
                        "[[result]] = Press any key to continue . . ."});
#line 32
 testRunner.And("the debug output as", ((string)(null)), table2401, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a blank cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteABlankCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a blank cmd", ((string[])(null)));
#line 36
this.ScenarioSetup(scenarioInfo);
#line 37
 testRunner.Given("I have this command script to execute \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 38
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 39
 testRunner.Then("the result of the command tool will be \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 40
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2402 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table2402.AddRow(new string[] {
                        "\"\""});
#line 41
 testRunner.And("the debug inputs as", ((string)(null)), table2402, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2403 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2403.AddRow(new string[] {
                        "[[result]] ="});
#line 44
 testRunner.And("the debug output as", ((string)(null)), table2403, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute invalid cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteInvalidCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute invalid cmd", ((string[])(null)));
#line 48
this.ScenarioSetup(scenarioInfo);
#line 49
 testRunner.Given("I have this command script to execute \"asdf\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 50
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 51
 testRunner.Then("the result of the command tool will be \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 52
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2404 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table2404.AddRow(new string[] {
                        "asdf"});
#line 53
 testRunner.And("the debug inputs as", ((string)(null)), table2404, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2405 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2405.AddRow(new string[] {
                        "[[result]] ="});
#line 56
 testRunner.And("the debug output as", ((string)(null)), table2405, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute cmd with negative recordset index")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteCmdWithNegativeRecordsetIndex()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute cmd with negative recordset index", ((string[])(null)));
#line 60
this.ScenarioSetup(scenarioInfo);
#line 61
 testRunner.Given("I have this command script to execute \"dir [[my(-1).dir]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 62
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 63
 testRunner.Then("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2406 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table2406.AddRow(new string[] {
                        "dir [[my(-1).dir]] ="});
#line 64
 testRunner.And("the debug inputs as", ((string)(null)), table2406, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2407 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2407.AddRow(new string[] {
                        "[[result]] ="});
#line 67
 testRunner.And("the debug output as", ((string)(null)), table2407, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a NULL cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteANULLCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a NULL cmd", ((string[])(null)));
#line 71
this.ScenarioSetup(scenarioInfo);
#line 72
 testRunner.Given("I have a command variable \"[[command]]\" equal to \"NULL\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 73
 testRunner.And("I have this command script to execute \"dir c:\\[[command]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 74
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 75
 testRunner.Then("the execution has \"No\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute a non existent variable cmd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteANonExistentVariableCmd()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute a non existent variable cmd", ((string[])(null)));
#line 78
this.ScenarioSetup(scenarioInfo);
#line 79
 testRunner.Given("I have this command script to execute \"dir c:\\[[command]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 80
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 81
 testRunner.Then("the result of the command tool will be \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 82
 testRunner.And("the execution has \"AN\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute commands with star notation")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteCommandsWithStarNotation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute commands with star notation", ((string[])(null)));
#line 84
this.ScenarioSetup(scenarioInfo);
#line 85
 testRunner.Given("I have a command variable \"[[coms().command]]\" equal to \"bob\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 86
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"dora\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 87
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"bill\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 88
 testRunner.And("I have a command variable \"[[results().res]]\" equal to \"res1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 89
 testRunner.And("I have a command result equal to \"[[results(*).res]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2408 = new TechTalk.SpecFlow.Table(new string[] {
                        "script"});
            table2408.AddRow(new string[] {
                        "echo [[coms(*).command]]"});
#line 90
 testRunner.And("I have these command scripts to execute in a single execution run", ((string)(null)), table2408, "And ");
#line 93
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 94
 testRunner.Then("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2409 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table2409.AddRow(new string[] {
                        "String = String"});
            table2409.AddRow(new string[] {
                        "String = String"});
            table2409.AddRow(new string[] {
                        "String = String"});
#line 95
 testRunner.And("the debug inputs as", ((string)(null)), table2409, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2410 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2410.AddRow(new string[] {
                        "[[results(1).res]] = bob"});
            table2410.AddRow(new string[] {
                        "[[results(2).res]] = dora"});
            table2410.AddRow(new string[] {
                        "[[results(3).res]] = bill"});
#line 100
 testRunner.And("the debug output as", ((string)(null)), table2410, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Execute commands with star notation to append")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "Command")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Scripting")]
        public virtual void ExecuteCommandsWithStarNotationToAppend()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute commands with star notation to append", ((string[])(null)));
#line 106
this.ScenarioSetup(scenarioInfo);
#line 107
 testRunner.Given("I have a command variable \"[[coms().command]]\" equal to \"bob\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 108
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"dora\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 109
 testRunner.And("I have a command variable \"[[coms().command]]\" equal to \"bill\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 110
 testRunner.And("I have a command variable \"[[results().res]]\" equal to \"res1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 111
 testRunner.And("I have a command result equal to \"[[results().res]]\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2411 = new TechTalk.SpecFlow.Table(new string[] {
                        "script"});
            table2411.AddRow(new string[] {
                        "echo [[coms(*).command]]"});
#line 112
 testRunner.And("I have these command scripts to execute in a single execution run", ((string)(null)), table2411, "And ");
#line 115
 testRunner.When("the command tool is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 116
 testRunner.Then("the execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2412 = new TechTalk.SpecFlow.Table(new string[] {
                        "Command"});
            table2412.AddRow(new string[] {
                        "String = String"});
            table2412.AddRow(new string[] {
                        "String = String"});
            table2412.AddRow(new string[] {
                        "String = String"});
#line 117
 testRunner.And("the debug inputs as", ((string)(null)), table2412, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2413 = new TechTalk.SpecFlow.Table(new string[] {
                        ""});
            table2413.AddRow(new string[] {
                        "[[results(4).res]] = bill"});
#line 122
 testRunner.And("the debug output as", ((string)(null)), table2413, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
