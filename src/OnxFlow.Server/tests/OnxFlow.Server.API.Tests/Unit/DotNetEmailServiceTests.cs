using System.Net.Mail;

namespace OnxFlow.Server.API.Tests.Unit;

public class DotNetEmailServiceTests
{
  private readonly Mock<IOptions<SmtpOptions>> _smtpOptionsMock = new();
  private readonly Mock<ILogger<DotNetEmailService>> _loggerMock = new();
  private readonly Mock<IEmailClient> _emailClientMock = new();

  private readonly DotNetEmailService _sut;

  public DotNetEmailServiceTests()
  {
    var smtpOptions = FakeDataFactory.SmtpOptions.Generate();

    _smtpOptionsMock
      .Setup(static s => s.Value)
      .Returns(smtpOptions);

    _sut = new DotNetEmailService(
      _smtpOptionsMock.Object,
      _loggerMock.Object,
      _emailClientMock.Object
    );
  }

  [Fact]
  public async Task SendEmailAsync_WhenEmailIsSentSuccessfully_ItShouldReturnOkResult()
  {
    var emailMessage = FakeDataFactory.EmailMessage.Generate();

    _emailClientMock
      .Setup(static e => e.SendMailAsync(It.IsAny<MailMessage>()))
      .Returns(Task.CompletedTask);

    var result = await _sut.SendEmailAsync(emailMessage);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SendEmailAsync_WhenEmailFailsToSendDueToSmtpException_ItShouldRetry()
  {
    var emailMessage = FakeDataFactory.EmailMessage.Generate();

    _emailClientMock
      .SetupSequence(static e => e.SendMailAsync(It.IsAny<MailMessage>()))
      .Throws<SmtpFailedRecipientException>()
      .Throws<SmtpException>()
      .Throws<SmtpFailedRecipientException>();

    var result = await _sut.SendEmailAsync(emailMessage);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().Contain(static e => e is EmailFailedError);

    _emailClientMock.Verify(static e => e.SendMailAsync(It.IsAny<MailMessage>()), Times.Exactly(3));
  }

  [Fact]
  public async Task SendEmailAsync_WhenEmailFailsToSend_ItShouldNotRetry()
  {
    var emailMessage = FakeDataFactory.EmailMessage.Generate();

    _emailClientMock
      .Setup(static e => e.SendMailAsync(It.IsAny<MailMessage>()))
      .Throws<Exception>();

    await _sut.SendEmailAsync(emailMessage);

    _emailClientMock.Verify(static e => e.SendMailAsync(It.IsAny<MailMessage>()), Times.Once());
  }
}