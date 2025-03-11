// -------------------------------------------------------------------------------------------------
//  <copyright file="HtmlReportCommandTestFixture.cs" company="Starion Group S.A.">
// 
//    Copyright 2019-2025 Starion Group S.A.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  ------------------------------------------------------------------------------------------------



namespace DEH_BSMI.Tools.Tests.Commands
{
    using System;
    using System.CommandLine.Invocation;

    using DEHBSMI.Tools.Commands;
    using DEHBSMI.Tools.Generators;

    using Moq;

    using NUnit.Framework;

    using STARIONGROUP.DEHCSV.Services;

    [TestFixture]
    public class HtmlReportCommandTestFixture
    {
        private Mock<IDataSourceSelector> dataSourceSelector;

        private Mock<IIterationReader> iterationReader;

        private Mock<IHtmlReportGenerator> htmlReportGenerator;

        private HtmlReportCommand.Handler handler;

        [SetUp]
        public void SetUp()
        {
            this.iterationReader = new Mock<IIterationReader>();
            this.dataSourceSelector = new Mock<IDataSourceSelector>();
            this.htmlReportGenerator = new Mock<IHtmlReportGenerator>();

            this.handler = new HtmlReportCommand.Handler(this.dataSourceSelector.Object, this.iterationReader.Object, this.htmlReportGenerator.Object);
        }

        [Test]
        public void Verify_that_ConvertCommand_Invoke_throws_exception()
        {
            var invocationContext = new InvocationContext(null);

            Assert.That(() =>
            {
                this.handler.Invoke(invocationContext);
            }, Throws.TypeOf<NotSupportedException>());
        }
    }
}
