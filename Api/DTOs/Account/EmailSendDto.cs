namespace Api.DTOs.Account
{
  public class EmailSendDto
  {
    //used to initialize property
    public EmailSendDto(string to, string subject, string body)
    {
      To = to;
      Subject = subject;
      Body = body;
    }

    //properties
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
  }
}
