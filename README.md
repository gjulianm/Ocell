## What is Ocell?

Ocell is a Twitter client for Windows Phone 7 and Windows 8. Supports multiple accounts, custom columns, and a whole lot of features more.

## How to compile

You just can't clone Ocell and compile. Before, you should do a few things.

* **SensitiveData.cs**. You will find that there is a file missing in some projects, called `SensitiveData.cs`. This contains all the API keys required to connect with Twitter and other services (currently Pocket). You should add your own keys. Links: [Twitter](https://dev.twitter.com/apps/new), [Pocket](http://getpocket.com/api/signup/).
* **External repos**. You must run ``git submodule init && git submodule update`` to get all the updates from the projects in submodules.

## Contribute

If you find a bug, see something that can be improved or you have better or new translations, just make a pull request. I'll be glad to take a look at it and accept it if it's good for the project :)