module FsTweet.Web.EmailService
open System.Net
open System.Net.Mail
open FsTweet.Domain.User

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

let sendActivationEmail activationUrl sendEmail (user : User) = 
  let emailTemplate = """
    Hi {username},   
    Your FsTweet account has been created successfully.   
    <a href="{link}"> Click here </a> to activate your account.
    
    Regards
    FsTweet
  """
  let body username userId = 
    emailTemplate
      .Replace("{username}", username)
      .Replace("{link}", activationUrl)

  let email = {
    Subject = "Your FsTweet account has been created"
    From = "email@fstweet.com"
    Destination = user.EmailAddress.Value
    Body = body user.Username.Value user.Id.Value.Value
    IsBodyHtml = true
  }
  sendEmail email
