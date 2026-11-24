using Drugly.DTO;
using Drugly.Server.Services;
using Drugly.Server.Services.Interfaces;
using Drugly.Server.Test.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;

namespace Drugly.Server.Test;

public class AuthorizationTests
{
    private static IAuthorizationService AuthorizationServiceFactory => CreateAuthService(TimeProvider.System);

    private static IAuthorizationService CreateAuthService(TimeProvider tp) => new AuthorizationService(tp);

    [Fact]
    public void TestCreateSession()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var patientId = Guid.NewGuid();
        var details = new AccountDetails(patientId, AccountType.Patient, "John", "Patient", "John@patient.com");

        //Act
        var session = authorizationService.CreateSession(details);

        //Assert
        Assert.Equal(patientId, session.AccountId);
        Assert.Equal(AccountType.Patient, session.AccountType);
        Assert.NotNull(session.SessionToken);
    }


    [Fact]
    public void TestSuccessfulDeleteSession()
    {
        //arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Patient, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //act
        var result = authorizationService.DeleteSession(mockHeader);

        //assert
        Assert.True(result);
    }

    [Fact]
    public void TestSessionNotFoundDeleteSession()
    {
        //arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Patient, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //act
        authorizationService.DeleteSession(mockHeader);
        var result = authorizationService.DeleteSession(mockHeader);

        //assert
        Assert.False(result);
    }

    [Fact]
    public void TestBadHeaderDeleteSession()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Patient, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("BadHeader", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //Act
        var result = authorizationService.DeleteSession(mockHeader);

        //assert
        Assert.False(result);
    }

    [Fact]
    public void TestNullTokenDeleteSession()
    {
        //arrange
        var authorizationService = AuthorizationServiceFactory;
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer ");
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //act
        var result = authorizationService.DeleteSession(mockHeader);

        //assert
        Assert.False(result);
    }

    [Fact]
    public void TestTokenNotFoundDeleteSession()
    {
        //arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Patient, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //act
        var deleted = authorizationService.DeleteSession(mockHeader);
        var result = authorizationService.DeleteSession(mockHeader);

        //assert
        Assert.True(deleted);
        Assert.False(result);
    }

    [Fact]
    public void TestSuccessfulUserAuthorizedPatient()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Patient, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //Act
        var result = authorizationService.IsUserAuthorized(mockHeader, AccountType.Patient);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void TestSuccessfulUserAuthorizedDoctor()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Doctor, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //Act
        var result = authorizationService.IsUserAuthorized(mockHeader, AccountType.Doctor);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void TestSuccessfulUserAuthorizedBoth()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Patient, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var detailsDoctor = new AccountDetails(Guid.NewGuid(), AccountType.Doctor, "John", "Doctor", "John@doctor.com");
        var sessionDoctor = authorizationService.CreateSession(detailsDoctor);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });
        var mockHeaderDoctor = Substitute.For<IHeaderDictionary>();
        var argDoctor = new StringValues("Bearer " + sessionDoctor.SessionToken);
        mockHeaderDoctor.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = argDoctor;
                return true;
            });

        //Act
        var result = authorizationService.IsUserAuthorized(mockHeader, [AccountType.Patient, AccountType.Doctor]);
        var result2 = authorizationService.IsUserAuthorized(mockHeaderDoctor, [AccountType.Patient, AccountType.Doctor]);

        //Assert
        Assert.True(result);
        Assert.True(result2);
    }

    [Fact]
    public void TestBadHeaderUserAuthorized()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Doctor, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("bad header", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //Act
        var result = authorizationService.IsUserAuthorized(mockHeader, AccountType.Doctor);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void TestNullTokenUserAuthorized()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Doctor, "John", "Patient", "John@patient.com");
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer ");
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //Act
        var result = authorizationService.IsUserAuthorized(mockHeader, AccountType.Doctor);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void TestExpiredTokenUserAuthorized()
    {
        //Arrange
        var tp = new TestTimeProvider();
        var authorizationService = CreateAuthService(tp);
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Doctor, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //Act
        tp.AddHours(5);
        var result = authorizationService.IsUserAuthorized(mockHeader, AccountType.Doctor);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void TestTokenNotFoundUserAuthorized()
    {
        //Arrange
        var authorizationService = AuthorizationServiceFactory;
        var details = new AccountDetails(Guid.NewGuid(), AccountType.Patient, "John", "Patient", "John@patient.com");
        var session = authorizationService.CreateSession(details);
        var mockHeader = Substitute.For<IHeaderDictionary>();
        var arg = new StringValues("Bearer " + session.SessionToken);
        mockHeader.TryGetValue("Authorization", out Arg.Any<StringValues>())
            .Returns(x =>
            {
                x[1] = arg;
                return true;
            });

        //Act
        authorizationService.DeleteSession(mockHeader);
        var result = authorizationService.IsUserAuthorized(mockHeader, AccountType.Patient);

        //Assert
        Assert.False(result);
    }
}
