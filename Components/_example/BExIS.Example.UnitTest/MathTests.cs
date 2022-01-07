using Moq;
using NUnit.Framework;

namespace BExIS.Example.UnitTest
{
    public class MathTests
    {
        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {

        }

        [SetUp]
        /// performs the initial setup for the tests. This runs once per test, NOT per class!
        protected void SetUp()
        {

        }

        [TearDown]
        /// performs the cleanup after each test
        public void TearDown()
        {

        }

        [OneTimeTearDown]
        /// It is called once after executing all the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the children
        /// Executes only if: counterpart OneTimeSetUp exists and executed successfully.
        public void OneTimeTearDown()
        {

        }

        [Test]
        public void Add_AInValid_returnMinusOne()
        {
            //Arrange
            int a = -1;
            int b = 4;

            Math math = new Math();

            //Act
            int result = math.Add(a, b);

            //Assert
            Assert.That(result, Is.EqualTo(-1));
        }

        
        [Test]
        public void Add_BIsInValid_returnMinusOne()
        {
            //Arrange
            int a = 0;
            int b = -1;

            Math math = new Math();

            //Act
            int result = math.Add(a, b);

            //Assert
            Assert.That(result, Is.EqualTo(-1));
        }

        [TestCase(1,1,2)]
        [TestCase(2,1,3)]
        [TestCase(3,1,4)]
        [TestCase(4,1,5)]
        [TestCase(5,1,6)]
        [TestCase(4,4,8)]
        [TestCase(10,23,33)]
        public void Add_AAndBValid_ResultMatchR(int a, int b, int r)
        {
            //Arrange
            Math math = new Math();

            //Act
            int result = math.Add(a, b);

            //Assert
            Assert.That(result, Is.EqualTo(r));
        }

        [TestCase(5,2,2.5)]
        public void Divide_A_With_B_ReturnC(int a, int b, double c)
        {
             var math = new Math();
           //Act
           double result = math.Divide(a,b);

           //Assert
           Assert.That(result, Is.EqualTo(c),"elli wrote it, write a email to her :D");
        }
        
        [TestCase(2,5)]
        public void Divide_getValuelessthenone_returnCaslessthenone(int a, int b)
        {
            var math = new Math();

            double result = math.Divide(a,b);

            Assert.That(result, Is.LessThan(1));
        }
        
        [TestCase(4,0,1)]
        [TestCase(2,2,4)]
        [TestCase(2,3,8)]
        public void POW_MH_ValidResult_return_C(int a,int b, int c)
        {
            var math = new Math();
            
            //Act
            int result = math.Pow_MH(a,b);

            //Assert
            Assert.That(result, Is.EqualTo(c));
        }

        [TestCase(4,0)]
        [TestCase(2,-2)]
        [TestCase(2,3)]
        public void POW_MH_ValidParameter_B(int a,int b)
        {
            //Arrange
            var math = new Math();

            //Act
            int result = math.Pow_MH(a,b);

            //Assert
            Assert.LessOrEqual(result,-1);
        }

        
        
        public void AddWithCheck_AAndBValid_ResultMatchR(int a, int b, int r)
        {
            //Arrange
            // create a mock of NumberHelper
            var numberHelperMock = new Mock<NumberHelper>();

            // setup the return of the function that are called in Math.AddWithCheck 
            numberHelperMock.Setup(n=>n.IsNumber(3)).Returns(true);

            // create a instance of Math with the mock numberhelper
            Math math = new Math(numberHelperMock.Object);

            //Act
            int result = math.AddWithCheck(a, b);

            //Assert
            Assert.That(result, Is.EqualTo(r));
        }

        [TestCase(0,1)]
        [TestCase(4,5)]
        [TestCase(4,7)]
        [TestCase(12,9)]
        [TestCase(16,15)]
        public void AddWithEvenCheck_AIsEvenAndBIsNotEven_returnMinusOne(int a, int b)
        {
            //Arrange
            Math math = new Math();

            //Act
            int result = math.AddWithEvenCheck(a, b);

            //Assert
            Assert.That(result, Is.EqualTo(-1));
        }

        [TestCase(4, 4, 16)]
        [TestCase(6, 6, 36)]
        public void Multiply_AandBvalid_returnC(int a, int b, int c)
        {

            Math math = new Math();
               

            int result = math.Multiply(a, b);

            // Assert 
            Assert.That(result, Is.EqualTo(c));
        }


        [TestCase(2,1,2)]
        [TestCase(2,2,4)]
        [TestCase(2,2,8)]
        [TestCase(3,2,9)]
        [TestCase(5,2,25)]
        [TestCase(99,6,941480149401)]
        public void POW_A_and_B_return_C(int a,int b, int c){
            Math math = new Math();

            int result=math.Pow_MH(a,b);

            Assert.That(result, Is.EqualTo(c));
        }

        [TestCase(2,-1)]
        [TestCase(4,-2)]
        [TestCase(99,-6)]
        public void POW_A_and_B_return_minusone(int a,int b){
            Math math = new Math();
            
            int result=math.Pow_MH(a,b);

            Assert.That(result, Is.EqualTo(-1));
        }
    
        [TestCase(2,0)]
        [TestCase(4,0)]
        [TestCase(99,0)]
        public void POW_A_and_B_return_one(int a,int b){
            Math math = new Math();

            int result=math.Pow_MH(a,b);

            Assert.That(result, Is.EqualTo(1));

        }
        
        [TestCase(-1, 4)]
        public void Multiply_AIsNull_returnNull(int a, int b)
        {
            var math = new Math();

            var result = math.Multiply(a, b);

            Assert.That(result, Is.EqualTo(-1));
        }

        [TestCase(0, 4)]
        public void Multiply_AIsZero_returnZero(int a, int b)
        {
            var math = new Math();

            var result = math.Multiply(a, b);

            Assert.That(result, Is.EqualTo(0));
        }
    }
}