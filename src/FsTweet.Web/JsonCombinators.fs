module FsTweet.Web.JsonCombinators

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Operators
open Suave.Successful
open Suave

let jsonSerializerSettings = 
  new JsonSerializerSettings(ContractResolver = new CamelCasePropertyNamesContractResolver())

let JSON v =       
  JsonConvert.SerializeObject(v, jsonSerializerSettings)
  |> OK 
  >=> Writers.setMimeType "application/json; charset=utf-8"

