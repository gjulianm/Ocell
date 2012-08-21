## What is Ocell?

Ocell is a Twitter client for Windows Phone 7 and Windows 8. Supports multiple accounts, custom columns, and a whole lot of features more.

## How to compile

You just can't clone Ocell and compile. Before, you should do a few things.

* **SensitiveData.cs**. You will find that there is a file missing in some projects, called `SensitiveData.cs`. This contains all the API keys required to connect with Twitter and other services (currently Pocket). You should add your own keys. Links: [Twitter][https://dev.twitter.com/apps/new], [Pocket][http://getpocket.com/api/signup/].
* **Hammock and Tweetsharp**. Although I initially used the original [Daniel Crenna's implementations][https://github.com/danielcrenna], I introduced some changes to work better with my app. I don't push them to the original repo because that would break his implementation, so I keep my own forks. You should clone them on Ocell.Common:

    cd Ocell.Common
    git clone git://github.com/gjulianm/tweetsharp.git
    git clone git://github.com/gjulianm/hammock.git

## Contribute

If you find a bug, see something that can be improved or you have better or new translations, just make a pull request. I'll be glad to take a look at it and accept it if it's good for the project :)