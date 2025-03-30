// -------------------------------------------------------------------------------------------------
//  <copyright file="XlReportGeneratorTestFixture.cs" company="Starion Group S.A.">
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

namespace DEH_BSMI.Reporting.Tests.Generators
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using DEHBSMI.Tools.Generators;

    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using STARIONGROUP.DEHCSV.Services;

    [TestFixture]
    public class XlReportGeneratorTestFixture
    {
        private ILoggerFactory loggerFactory;

        private IterationReader iterationReader;

        private XlReportGenerator XlReportGenerator;

        private Uri uri;

        [SetUp]
        public void SetUp()
        {
            this.loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            this.iterationReader = new IterationReader(this.loggerFactory);

            this.XlReportGenerator = new XlReportGenerator(this.loggerFactory);
        }

        [Test]
        public async Task Verify_that_dndnanusv_model_can_be_written_to_xl_file()
        {
            var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "bsmi.zip");

            this.uri = new Uri(path);

            var credentials = new Credentials("admin", "pass", uri);

            var session = new Session(jsonFileDal, credentials, new CDPMessageBus());

            await session.Open(false);

            var iteration = await this.iterationReader.ReadAsync(session, "BSMI", 1, "SYS");

            var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "BSMI.xlsx");
            var outputReport = new FileInfo(outputPath);

            Assert.That(() => this.XlReportGenerator.Generate(iteration, iteration.RequirementsSpecification, outputReport, "9999"), Throws.Nothing);
        }
    }
}
