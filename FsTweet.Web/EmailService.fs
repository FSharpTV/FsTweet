module FsTweet.Web.EmailService
open System.Net
open System.Net.Mail

type SmtpConfig = {
  Username : string
  Password : string
  Host : string 
  Port : int
}

type Email = {
  Subject : string
  From : string
  Destination : string
  Body : string
  IsBodyHtml : bool
}

let sendEmail onEmailSent config email =  
  
  use mail = new MailMessage(email.From, email.Destination, email.Subject, email.Body)
  mail.IsBodyHtml <- email.IsBodyHtml
  let clientCredentials = new NetworkCredential(config.Username, config.Password)
  
  let smtpClient = new SmtpClient(config.Host,config.Port)
  smtpClient.EnableSsl <- true
  smtpClient.UseDefaultCredentials <- false
  smtpClient.Credentials <- clientCredentials
  smtpClient.SendCompleted.Add onEmailSent
  smtpClient.SendAsync(mail, null)
