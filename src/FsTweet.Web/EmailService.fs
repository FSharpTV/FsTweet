module FsTweet.Web.EmailService
open System.Net
open System.Net.Mail
open FsTweet.Domain.User
open System

type Email = {
  Subject : string
  From : string
  Destination : string
  Body : string
  IsBodyHtml : bool
}
let sendActivationEmail activationUrl sendEmail user = 
  let emailTemplate (userName:Username) (link:Uri) = 
    sprintf 
      """
      Hi %s,   
      Your FsTweet account has been created successfully.   
      <a href="%O"> Click here </a> to activate your account.
      
      Regards
      FsTweet""" userName.Value link 
  sendEmail
   { Subject = "Your FsTweet account has been created"
     From = "email@fstweet.com"
     Destination = user.EmailAddress.Value.Value
     Body = emailTemplate user.Username activationUrl
     IsBodyHtml = true } 
