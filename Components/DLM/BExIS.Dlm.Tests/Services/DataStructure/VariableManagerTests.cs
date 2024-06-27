using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Tests.Services.DataStructure
{
    public class VariableManagerTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [SetUp]
        protected void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Meaning> repoM = uow.GetRepository<Meaning>();
                IRepository<VariableTemplate> repoVT = uow.GetRepository<VariableTemplate>();
                IRepository<VariableInstance> repoVI = uow.GetRepository<VariableInstance>();
                IRepository<StructuredDataStructure> repoSDS = uow.GetRepository<StructuredDataStructure>();

                repoM.Get().ToList().ForEach(m => repoM.Delete(m));
                repoVT.Get().ToList().ForEach(v => repoVT.Delete(v));
                repoVI.Get().ToList().ForEach(v => repoVI.Delete(v));

                repoSDS.Get().ToList().ForEach(s => repoSDS.Delete(s));

                uow.Commit();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test()]
        public void CreateVariableTemplate_Valid_VariableTemplate()
        {
            using (var variableManager = new VariableManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Act
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                //Assert

                Assert.IsNotNull(variableTemplate);
                Assert.That(variableTemplate.Id > 0);

                var variableTemplateFromDB = variableManager.GetVariableTemplate(variableTemplate.Id);

                Assert.AreSame(variableTemplate, variableTemplateFromDB);
            }
        }

        [Test()]
        public void CreateVariableTemplate_NameIsEmpty_ArgumentNullException()
        {
            using (var variableManager = new VariableManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() =>

                variableManager.CreateVariableTemplate(
                    "",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    )
                );

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("name"));
            }
        }

        [Test()]
        public void CreateVariableTemplate_DataTypeIsNull_ArgumentNullException()
        {
            using (var variableManager = new VariableManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() =>

                variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    null,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    )
                );

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("dataType"));
            }
        }

        [Test()]
        public void CreateVariableTemplate_WithNewMeaning_VariableTemplate()
        {
            using (var variableManager = new VariableManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            using (var meaningManager = new MeaningManager())
            {
                //Arrange
                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var meaning = new Meaning();
                meaning.Name = "Test";
                meaning.Description = "Test";
                meaning.Selectable = true;
                meaning = meaningManager.addMeaning(meaning);

                var result = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz",
                    "",
                    new List<Meaning>() { meaning }
                    ); ;

                //Assert
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test()]
        public void UpdateVariableTemplate_ValidParameters_UpdatedVariableTemplate()
        {
            using (var variableManager = new VariableManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                // create a variableTemplate
                var changedVariableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // change label
                changedVariableTemplate.Label = "updated";

                //Act
                var updatedVariableTemplate = variableManager.UpdateVariableTemplate(changedVariableTemplate);

                //Assert

                Assert.IsNotNull(changedVariableTemplate);
                Assert.That(changedVariableTemplate.Id > 0);
                Assert.IsNotNull(updatedVariableTemplate);
                Assert.That(updatedVariableTemplate.Id > 0);

                var variableTemplateFromDB = variableManager.GetVariableTemplate(updatedVariableTemplate.Id);

                Assert.AreEqual(changedVariableTemplate.Label, variableTemplateFromDB.Label);
            }
        }

        [Test()]
        public void DeleteVariableTemplate_NotExist_returnTrue()
        {
            using (var variableManager = new VariableManager())
            {
                //Arrange
                //Act
                var ex = Assert.Throws<ArgumentNullException>(() =>
                variableManager.DeleteVariableTemplate(1000000));

                //Assert
                Assert.That(ex.Message.Contains("Attempt to create delete event with null entity"));
            }
        }

        [Test()]
        public void DeleteVariableTemplate_IdIsZero_returnTrue()
        {
            using (var variableManager = new VariableManager())
            {
                //Arrange
                //Act
                var ex = Assert.Throws<ArgumentException>(() =>
                variableManager.DeleteVariableTemplate(0));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("id"));
                Assert.That(ex.Message.Contains("Id must be greater then 0."));
            }
        }

        [Test()]
        public void DeleteVariableTemplate_Exist_returnTrue()
        {
            using (var variableManager = new VariableManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                // create a variableTemplate
                var changedVariableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                //Act
                var result = variableManager.DeleteVariableTemplate(changedVariableTemplate.Id);

                //Assert

                Assert.IsTrue(result);
            }
        }

        [Test()]
        public void DeleteVariableTemplate_WithMeaning_true()
        {
            using (var variableManager = new VariableManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            using (var meaningManager = new MeaningManager())
            {
                //Arrange
                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var meaning = new Meaning();
                meaning.Name = "Test";
                meaning.Description = "Test";
                meaning.Selectable = true;
                meaning = meaningManager.addMeaning(meaning);

                var varTemp = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz",
                    "",
                    new List<Meaning>() { meaning }
                    );

                // act
                var result = variableManager.DeleteVariableTemplate(varTemp.Id);

                //Assert
                Assert.That(result, Is.EqualTo(true));
            }
        }

        [Test()]
        public void CreateVariable_Valid_Variable()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                //Act
                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    dataStructure.Id,
                    variableTemplate.Id
                    );

                //Assert
                Assert.IsNotNull(variable);
                Assert.That(variable.Id > 0);

                var variableFromDB = variableManager.GetVariable(variable.Id);

                Assert.AreSame(variable, variableFromDB);
            }
        }

        [Test()]
        public void CreateVariable_WithTemplate_Variable()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                //Act
                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    dataStructure.Id,
                    variableTemplate.Id
                    );

                //Assert
                Assert.IsNotNull(variable);
                Assert.That(variable.Id > 0);

                var variableFromDB = variableManager.GetVariable(variable.Id);

                Assert.AreSame(variable, variableFromDB);
            }
        }

        [Test()]
        public void CreateVariable_NameIsEmptOrNull_ArgumentNullException()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() => variableManager.CreateVariable(
                    "",
                    variableTemplate.DataType,
                    variableTemplate.Unit,
                    dataStructure.Id,
                    true,
                    false,
                    1,
                    variableTemplate.Id
                    ));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("name"));
                Assert.That(ex is ArgumentNullException);
            }
        }

        [Test()]
        public void CreateVariable_DataTypeIsNull_ArgumentNullException()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() => variableManager.CreateVariable(
                   "Test Variable",
                   null,
                   variableTemplate.Unit,
                   dataStructure.Id,
                   true,
                   false,
                   1,
                   variableTemplate.Id
                   ));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("dataType"));
                Assert.That(ex is ArgumentNullException);
            }
        }

        [Test()]
        public void CreateVariable_DataSturctureIdIsZero_ArgumentNullException()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() => variableManager.CreateVariable(
                   "Test Variable",
                   variableTemplate.DataType,
                   variableTemplate.Unit,
                   0,
                   true,
                   false,
                   1,
                   variableTemplate.Id
                   ));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("dataStructureId"));
                Assert.That(ex is ArgumentNullException);
            }
        }

        [Test()]
        public void CreateVariable_WithMissingValues_Variable()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            using (var missingValueManager = new MissingValueManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                // generate list of missing values
                List<MissingValue> missingValues = new List<MissingValue>();
                var ms = new MissingValue()
                {
                    DisplayName = "test",
                    Description = "test",
                    Placeholder = "test"
                };

                missingValues.Add(ms);

                //Act
                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    dataType,
                    unit,
                    dataStructure.Id,
                    true,
                    true,
                    1,
                    0,
                    "",
                    "",
                    "",
                    0,
                    missingValues
                    );

                //Assert
                Assert.IsNotNull(variable);
                Assert.That(variable.Id > 0);
                Assert.That(variable.MissingValues.Any());

                var variableFromDB = variableManager.GetVariable(variable.Id);

                var msl = missingValueManager.Repo.Get().Where(x => x.Id == variable.MissingValues.First().Id).FirstOrDefault();

                Assert.IsNotNull(msl);

                Assert.AreSame(variable, variableFromDB);
                Assert.That(variableFromDB.MissingValues.Count > 0);
            }
        }

        [Test()]
        public void CreateVariable_WithConstraints_Variable()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            using (var constraintManager = new ConstraintManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                // generate a constraint
                List<Constraint> constraints = new List<Constraint>();

                var c = new DomainConstraint();
                c.Name = "test";
                c.Description = "test description";
                c.Items.Add(new DomainItem() { Key = "a", Value = "a" });

                c = constraintManager.Create(c);
                constraints.Add(c);

                //Act
                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    dataType,
                    unit,
                    dataStructure.Id,
                    true,
                    true,
                    1,
                    0,
                    "",
                    "",
                    "",
                    0,
                    null,
                    constraints.Select(co => co.Id).ToList()
                    ); ;

                //Assert
                Assert.IsNotNull(variable);
                Assert.That(variable.Id > 0);
                Assert.That(variable.VariableConstraints.Any());

                var variableFromDB = variableManager.GetVariable(variable.Id);

                var cons = constraintManager.ConstraintRepository.Get().Where(x => x.Id == variable.VariableConstraints.First().Id).FirstOrDefault();

                Assert.IsNotNull(cons);

                Assert.AreSame(variable, variableFromDB);
                Assert.That(variableFromDB.VariableConstraints.Count > 0);
            }
        }

        [Test()]
        public void CreateVariable_2VarsWithSameConstraints_Variable()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            using (var constraintManager = new ConstraintManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                // generate a constraint
                List<Constraint> constraints = new List<Constraint>();

                var c = new DomainConstraint();
                c.Name = "test";
                c.Description = "test description";
                c.Items.Add(new DomainItem() { Key = "a", Value = "a" });

                c = constraintManager.Create(c);
                constraints.Add(c);

                //Act
                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    dataType,
                    unit,
                    dataStructure.Id,
                    true,
                    true,
                    1,
                    0,
                    "",
                    "",
                    "",
                    0,
                    null,
                    constraints.Select(co => co.Id).ToList()
                    );

                dataStructure = datastructureManager.AddVariable(dataStructure.Id, variable.Id);

                var variable2 = variableManager.CreateVariable(
                   "TestVariable",
                   dataType,
                   unit,
                   dataStructure.Id,
                   true,
                   true,
                   1,
                   0,
                   "",
                   "",
                   "", 0,
                   null,
                   constraints.Select(co => co.Id).ToList()
                   );

                dataStructure = datastructureManager.AddVariable(dataStructure.Id, variable2.Id);

                //Assert
                Assert.IsNotNull(variable);
                Assert.That(variable.Id > 0);
                Assert.That(variable.VariableConstraints.Any());

                var variableFromDB = variableManager.GetVariable(variable.Id);

                var cons = constraintManager.ConstraintRepository.Get().Where(x => x.Id == variable.VariableConstraints.First().Id).FirstOrDefault();

                Assert.IsNotNull(cons);

                Assert.AreSame(variable, variableFromDB);
                Assert.That(variableFromDB.VariableConstraints.Count > 0);
            }
        }

        [Test()]
        public void CreateVariable_WithoutVariableTemplate_Variable()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                //Act
                var result = variableManager.CreateVariable(
                   "Test Variable",
                   dataType,
                   unit,
                   dataStructure.Id,
                   true,
                   false,
                   1
                   );

                //Assert
                Assert.NotNull(result);
            }
        }

        [Test()]
        public void UpdateVariable_ValidParameters_UpdatedVariable()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                var created = variableManager.CreateVariable(
                    "TestVariable",
                    variableTemplate.DataType,
                    variableTemplate.Unit,
                    dataStructure.Id,
                    true,
                    false,
                    1,
                    variableTemplate.Id
                    );

                //Act
                created.Label = "updated";

                var updated = variableManager.UpdateVariable(created);

                //Assert
                Assert.IsNotNull(created);
                Assert.That(created.Id > 0);

                Assert.IsNotNull(updated);
                Assert.That(updated.Id > 0);

                var variableFromDB = variableManager.GetVariable(created.Id);

                Assert.AreEqual(updated.Label, variableFromDB.Label);
            }
        }

        [Test()]
        public void DeleteVariable_Exist_ReturnTrue()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    variableTemplate.DataType,
                    variableTemplate.Unit,
                    dataStructure.Id,
                    true,
                    true,
                    1,
                    variableTemplate.Id
                    );

                //Act
                var result = variableManager.DeleteVariable(variable.Id);

                //Assert
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void DeleteVariable_WithMeaningAndConstraints_ReturnTrue()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            using (var meaningManager = new MeaningManager())
            using (var constraintManager = new ConstraintManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                //meaning
                var meaning = meaningManager.addMeaning(
                    "TestMeaning",
                    "",
                    "",
                    true,
                    true,
                    new List<MeaningEntry>(),
                    new List<long>(),
                    new List<long>()
                    );

                List<long> meaningIds = new List<long>();
                meaningIds.Add(meaning.Id);

                //constraint
                var constraint = constraintManager.Create(new DomainConstraint()
                {
                    Name = "TestConstraint",
                    Description = ""
                });
                List<long> constraintIds = new List<long>();
                constraintIds.Add(constraint.Id);

                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    variableTemplate.DataType,
                    variableTemplate.Unit,
                    dataStructure.Id,
                    true,
                    true,
                    1,
                    variableTemplate.Id,
                    "",
                    "",
                    "",
                    0,
                    null,
                    constraintIds,
                    meaningIds);

                //Act
                var result = variableManager.DeleteVariable(variable.Id);

                //Assert
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void DeleteVariable_ConstraintsBindedMultiTimes_ReturnTrue()
        {
            using (var variableManager = new VariableManager())
            using (var datastructureManager = new DataStructureManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            using (var meaningManager = new MeaningManager())
            using (var constraintManager = new ConstraintManager())
            {
                //Arrange
                var dataType = dataTypeManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(dataType, "datatype not exist");

                var unit = unitManager.Repo.Get().FirstOrDefault();
                Assert.IsNotNull(unit, "unit not exist");

                //Variable Template
                var variableTemplate = variableManager.CreateVariableTemplate(
                    "TestVariableTemplate",
                    dataType,
                    unit,
                    "TestVariableTemplate Description",
                    "xyz"
                    );

                // Datastructure
                var dataStructure = datastructureManager.CreateStructuredDataStructure(
                    "Test Structure",
                    "Test StrutcureDescription",
                    null,
                    null,
                    DataStructureCategory.Generic,
                    null
                    );

                //meaning
                var meaning = meaningManager.addMeaning(
                    "TestMeaning",
                    "",
                    "",
                    true,
                    true,
                    new List<MeaningEntry>(),
                    new List<long>(),
                    new List<long>()
                    );

                List<long> meaningIds = new List<long>();
                meaningIds.Add(meaning.Id);

                //constraint
                var constraint = constraintManager.Create(new DomainConstraint()
                {
                    Name = "domainTest",
                    Description = "domainTest"
                });
                List<long> constraintIds = new List<long>();
                constraintIds.Add(constraint.Id);

                var variable = variableManager.CreateVariable(
                    "TestVariable",
                    variableTemplate.DataType,
                    variableTemplate.Unit,
                    dataStructure.Id,
                    true,
                    true,
                    1,
                    variableTemplate.Id,
                    "",
                    "",
                    "", 0,
                    null,
                    constraintIds,
                    meaningIds
                    );

                var variable2 = variableManager.CreateVariable(
                    "TestVariable2",
                    variableTemplate.DataType,
                    variableTemplate.Unit,
                    dataStructure.Id,
                    true,
                    true,
                    1,
                    variableTemplate.Id,
                    "",
                    "",
                    "",
                    0,
                    null,
                    constraintIds,
                    meaningIds);

                //Act
                var result = variableManager.DeleteVariable(variable.Id);

                //Assert
                Assert.IsTrue(result);
            }
        }

        [Test()]
        public void DeleteVariable_NotExist_ArgumentNullException()
        {
            using (var variableManager = new VariableManager())
            {
                //Arrange
                //Act
                var ex = Assert.Throws<ArgumentNullException>(() =>
                variableManager.DeleteVariable(1000000));

                //Assert
                Assert.That(ex.Message.Contains("Attempt to create delete event with null entity"));
            }
        }

        [Test()]
        public void DeleteVariable_IdIsZero_ReturnTrue()
        {
            using (var variableManager = new VariableManager())
            {
                //Arrange
                //Act
                var ex = Assert.Throws<ArgumentException>(() =>
                variableManager.DeleteVariable(0));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("id"));
                Assert.That(ex is ArgumentException);
            }
        }
    }
}