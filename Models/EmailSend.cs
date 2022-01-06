using MailKit.Net.Smtp;
using MimeKit;

namespace ASp.netCore_empty_tutorial.Models
{
    public class EmailSend
    {
        //email send with MailKit and Googleservices
        public void EmailConfirm(string confirmationLink, string userName, string userEmail)
        {
            using (SmtpClient client = new SmtpClient())
            {

                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("testApp", "testApp@asd.com"));
                message.To.Add(new MailboxAddress(userName, userEmail));
                message.Subject = $"Confirm Email for {userName}";
                message.Body = new BodyBuilder() { HtmlBody = $"<h5 style=\"color:black;\">Confirm User Registration,click the link:</h5><a href=\"{confirmationLink}\">Click here!</a>" }.ToMessageBody();

                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("testingemail@gmailcom", "xxxxxpassword");
                client.Send(message);

                client.Disconnect(true);
            }
           
        }
    }
}
