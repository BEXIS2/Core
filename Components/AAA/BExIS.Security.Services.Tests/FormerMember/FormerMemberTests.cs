using BExIS.App.Testing;
using BExIS.Modules.SAM.UI.Helpers;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using NUnit.Framework;

namespace BExIS.Security.Services.Tests.FormerMember
{
    [TestFixture]
    public class FormerMemberTests
    {
        private TestSetupHelper helper = null;
        private User user;
        private Group group;

        [SetUp]
        protected void SetUp()
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            var userManager = new UserManager();
            var groupManager = new GroupManager();

            Group newGroup = new Group();
            User newUser = new User();

            newGroup.Name = "alumni";

            newUser.UserName = "TestUser";

            try
            {
                userManager.CreateAsync(newUser);
                groupManager.CreateAsync(newGroup);
                user = userManager.FindByNameAsync(newUser.Name).Result;
                group = groupManager.FindByNameAsync(newGroup.Name).Result;

            }
            catch
            {

            }
            finally
            {
                userManager.Dispose();
                groupManager.Dispose(); 

            }
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
            using (var userManager = new UserManager())
            using (var groupManager = new GroupManager())
            {
                userManager.DeleteAsync(user);
                groupManager.DeleteAsync(group);
            }
               
        }

        [Test]
        public void IsFormerMember_UserIsNotFormerMember_ReturnFalse()
        {
            //Arrange
            //Arrange
            using (var identityUserService = new IdentityUserService())
            {
                if (group.Users.Contains(user))
                {
                    identityUserService.RemoveFromRoleAsync(user.Id, group.Name);
                }
            }

            //Act
            var result = FormerMemberStatus.IsFormerMember(user.Id, group.Name);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFormerMember_UserIsFormerMember_ReturnTrue()
        {
            //Arrange
            using (var groupManager = new GroupManager())
            {
                if (!group.Users.Contains(user))
                {
                    group.Users.Add(user);
                    groupManager.UpdateAsync(group);
                }
            }

            //Act
            var result = FormerMemberStatus.IsFormerMember(user.Id, group.Name);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ChangeToFormerMember_UserIsFormerMember_ReturnFalse()
        {
            //Arrange
            using (var groupManager = new GroupManager())
            {
                if (!group.Users.Contains(user))
                {
                    group.Users.Add(user);
                    groupManager.UpdateAsync(group);
                }
            }

            //Act
            var result = FormerMemberStatus.ChangeToFormerMember(user, group.Name);


          //Assert
          Assert.IsFalse(result);

        }

        [Test]
        public void ChangeToFormerMember_UserIsNotFormerMember_ReturnTrue()
        {
            //Arrange
            using (var groupManager = new GroupManager())
            {
                if (group.Users.Contains(user))
                {
                    group.Users.Remove(user);
                    groupManager.UpdateAsync(group);
                }
            }

            //Act
            var result = FormerMemberStatus.ChangeToFormerMember(user, group.Name);


            //Assert
            Assert.IsTrue(result);

        }


        [Test]
        public void ChangeStatusToNonFormerMember_UserIsFormerMember_ReturnTrue()
        {
            //Arrange
            using (var groupManager = new GroupManager())
            {
                if (!group.Users.Contains(user))
                {
                    group.Users.Add(user);
                    groupManager.UpdateAsync(group);
                }
            }

            //Act
            var result = FormerMemberStatus.ChangeToNonFormerMember(user, group.Name);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ChangeStatusToNonFormerMember_UserIsNotFormerMember_ReturnFalse()
        {
            //Arrange
            using (var identityUserService = new IdentityUserService())
            {
                if (group.Users.Contains(user))
                {
                    group.Users.Remove(user);
                    identityUserService.RemoveFromRoleAsync(user.Id, group.Name);
                }
            }

            //Act
            var result = FormerMemberStatus.ChangeToNonFormerMember(user, group.Name);

            //Assert
            Assert.IsFalse(result);
        }


    }
}
