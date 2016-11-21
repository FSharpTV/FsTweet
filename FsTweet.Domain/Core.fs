module FsTweet.Domain.Core

type Error =
| PersistenceError of string
| RequestError of string