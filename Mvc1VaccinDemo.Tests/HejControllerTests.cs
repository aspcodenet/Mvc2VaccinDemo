using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mvc1VaccinDemo.Controllers;
using Mvc1VaccinDemo.Services.Krisinformation;
using Mvc1VaccinDemo.ViewModels;

namespace Mvc1VaccinDemo.Tests
{
    public class BaseTest
    {
        protected AutoFixture.Fixture fixture = new AutoFixture.Fixture();

    }

    [TestClass]
    public class HejControllerTests : BaseTest
    {
        private HejController sut;
        private Mock<IKrisInfoService> krisInfoServiceMock;
        private KrisInfo info; 

        public HejControllerTests()
        {


            info = fixture.Create<KrisInfo>();


            //info = new KrisInfo
            //{
            //    Displaymode = 0,
            //    Emergency = false,
            //    LinkUrl = "1",
            //    Id = "32132321",
            //    ImageUrl = "3213",
            //    Text = "23321321123312",
            //    Title = "231"
            //};
            krisInfoServiceMock = new Mock<IKrisInfoService>();
            krisInfoServiceMock.Setup(r => r.GetKrisInformation(It.IsAny<string>())).Returns(info);
            sut = new HejController(krisInfoServiceMock.Object);
        }

        [TestMethod]
        public void WhenShowIsCalledKrisInfoServicedIsCalledWithCorrectId()
        {
            string id = "123";

            sut.Show(id);
            //Verifiera att _service.GHetKLrisInfo 123
            krisInfoServiceMock.Verify(r=>r.GetKrisInformation(id),Times.Once);
        }


        [TestMethod]
        public void When12345ExceptionShouldBeThrowm()
        {
            string id = "12345";

            Assert.ThrowsException<InvalidDataException>(() => sut.Show(id));
        }



        [TestMethod]
        public void WhenShowIsCalledViewModelIsCorrectlyPopulated()
        {
            string id = "123";

            var result = sut.Show(id);

            var viewResult = result as ViewResult;
            var model = viewResult.ViewData.Model as ArticleViewModel;

            Assert.AreEqual(info.Text,model.Text);
            Assert.AreEqual(info.LinkUrl, model.LinkUrl);
            Assert.AreEqual(info.ImageUrl, model.ImageUrl);
            Assert.AreEqual(info.Displaymode, model.Displaymode);
            Assert.AreEqual(info.Emergency, model.Emergency);
            Assert.AreEqual(info.Title, model.Title);


            //Verifiera att ViewModel.Text = info.Text
        }



    }
}
