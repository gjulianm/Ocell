using System.Collections.Generic;
using System.Linq;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public static class DMExtensions
    {
        public static IEnumerable<GroupedDM> Group(this IEnumerable<TwitterDirectMessage> list, UserToken user)
        {
            return list
                .GroupBy(x => x.GetPairName(user))
                .Select(x => new GroupedDM(x, user));
        }

        public static IEnumerable<TwitterDirectMessage> Ungroup(this IEnumerable<GroupedDM> list)
        {
            return list.Select(x => x.Messages).
                Aggregate((IEnumerable<TwitterDirectMessage>)(new List<TwitterDirectMessage>()),
                (acc, x) => acc.Union(x));
        }

        public static IEnumerable<GroupedDM> Regroup(this IEnumerable<GroupedDM> list, IEnumerable<TwitterDirectMessage> messages, UserToken user)
        {
            return list.Ungroup().Union(messages).Group(user);
        }

        public static string GetPairName(this TwitterDirectMessage msg, UserToken user)
        {
            if (msg.SenderId == user.Id)
                return msg.RecipientScreenName;
            else
                return msg.SenderScreenName;
        }
    }
}
