using System;
using System.Collections.Generic;
using JavascriptViewResultSample.Models;
using JavascriptViewResultSample.Components.Paging;
using System.Linq;

namespace JavascriptViewResultSample.Services.Data
{
    public class InMemoryDataService
    {
        private static int _lipsumStart;

        public static PagedList<RegisteredUser> GetAll(Pager pager)
        {
            IQueryable<RegisteredUser> query = GetAll<RegisteredUser>();
            IQueryable<RegisteredUser> pagedQuery = query.Skip(pager.First).Take(pager.PerPage);

            return new PagedList<RegisteredUser>(pager, pagedQuery.ToList(), query.Count());
        }
        
        public static T Get<T>(Guid id) where T : RegisteredUser
        {
            T result = GetAll<T>().FirstOrDefault(item => item.Id == id);
            return result;
        }
        
        public static IQueryable<T> GetAll<T>()
        {
            string type = typeof(T).Name;
            switch (type)
            {
                case "RegisteredUser":
                    List<RegisteredUser> list = GenerateSomeRegisteredUsers(85);
                    return (IQueryable<T>)list.AsQueryable();
            }
            return new List<T>().AsQueryable();
        }
        
        public static List<RegisteredUser> GenerateSomeRegisteredUsers(int howMany)
        {
            _lipsumStart = 0;
            var RegisteredUsers = new List<RegisteredUser>();
            for (int i = 0; i < howMany; i++)
            {
                Guid guid = Guid.NewGuid();
                RegisteredUsers.Add(new RegisteredUser
                {
                    Id = guid,
                    Name = GetLipsumSample(),
                    Bio = string.Format("He be {0}", guid),
                    Avatar = string.Format("nature_{0}{1}.gif", i < 9 ? "0" : string.Empty, i + 1)
                });
            }

            return RegisteredUsers;
        }
        
        private static string GetLipsumSample()
        {
            var all = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris posuere ultrices tristique. Fusce volutpat nibh at lorem imperdiet non scelerisque erat dignissim. 
Nam in justo purus. Aenean sit amet mattis odio. Sed pellentesque consectetur nulla, et pulvinar lacus mollis a. Phasellus aliquam tortor ac libero rutrum id ullamcorper eros sodales. 
Donec tincidunt rhoncus pellentesque. Maecenas feugiat ligula ut mi dignissim condimentum. Cras laoreet ligula at lacus accumsan at posuere justo vehicula. Vestibulum a nibh in risus vulputate 
feugiat nec a nulla. Donec tincidunt turpis quis purus sollicitudin at placerat libero consectetur. Donec non purus nec elit egestas lobortis eu vitae odio. Donec sit amet urna accumsan magna
hendrerit scelerisque. Fusce vehicula adipiscing iaculis. Curabitur accumsan convallis leo, sit amet consectetur nisl condimentum a. Integer sodales libero nec urna euismod ut aliquam lectus
aliquet. Donec vel neque urna. Fusce ultrices bibendum commodo. Curabitur imperdiet mattis ligula ut aliquam. Maecenas in augue quis ipsum accumsan sagittis.Donec sagittis nunc et enim laoreet
volutpat. Curabitur sollicitudin diam commodo purus euismod dapibus. Nam consectetur quam sit amet dui lacinia non bibendum mauris ornare. Morbi laoreet felis ac purus convallis id dapibus 
mauris aliquam. Nam cursus adipiscing ligula ac auctor. Donec ut nisl elit. Pellentesque vitae turpis non mi placerat dignissim. Pellentesque blandit bibendum dui, a luctus risus vulputate eget. 
In hac habitasse platea dictumst. Aenean adipiscing, ante vel rutrum viverra, mi augue adipiscing libero, ac porta est ante ac sapien. In commodo quam at nisl posuere a varius ligula congue. 
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Vestibulum scelerisque commodo gravida. Aliquam erat volutpat. Duis vel convallis quam.Nullam eget 
justo est, at auctor turpis. Quisque pharetra volutpat justo in accumsan. Sed et porttitor augue. Fusce vestibulum placerat eleifend. Etiam fringilla urna quis tellus viverra pretium. Donec 
porta dolor eget lectus tristique a pharetra diam tempus. Vivamus gravida ligula ut neque sollicitudin ac dapibus metus tempus. Suspendisse dictum tellus in erat euismod molestie. Fusce vehicula, 
elit ut tincidunt iaculis, magna odio sagittis magna, non euismod metus augue vitae magna. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur
adipiscing elit. Nulla erat justo, condimentum id rhoncus vel, imperdiet quis lorem. Nam orci tellus, adipiscing at sagittis eu, cursus at erat. Maecenas rutrum justo id sem vulputate non
faucibus sapien aliquet. Sed eget tortor a urna suscipit pulvinar.Fusce dapibus venenatis fringilla. Nullam vel tortor sapien. Integer a libero vel massa mattis cursus. Class aptent taciti 
sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur consequat felis ac magna scelerisque hendrerit. Donec ornare purus augue. Aenean gravida dui quis enim feugiat 
quis sagittis libero scelerisque. Ut iaculis lorem eu magna venenatis dapibus. Curabitur massa tortor, accumsan eget pellentesque sed, blandit in tortor. Duis ac sapien augue. Nunc dapibus, nisi 
sit amet commodo mollis, augue dolor hendrerit sapien, suscipit rhoncus lectus massa et dui. Vestibulum tincidunt, nunc id rutrum sodales, nunc sem aliquet nisl, non lobortis urna ligula eu erat.
Integer tristique orci ac massa cursus sed adipiscing quam egestas. Maecenas bibendum odio vitae massa volutpat quis tristique eros sodales. Nulla et dignissim orci. Vestibulum quis ligula leo.Ut
non arcu et lacus aliquet auctor quis non velit. Praesent auctor sagittis magna, eget luctus magna egestas a. Ut ac tincidunt urna. Aenean et nibh diam. In nulla diam, faucibus ac cursus id,
eleifend ac felis. Donec viverra interdum congue. Suspendisse potenti. Aliquam metus sapien, tincidunt a facilisis sed, vulputate congue massa. Integer condimentum lorem sapien, vitae gravida
magna. Suspendisse cursus, nibh non interdum pulvinar, purus est convallis tellus, a tincidunt nisl risus vel turpis. Vestibulum pharetra, dui ac lacinia semper, justo leo fermentum mauris, at 
ullamcorper nisl ante vitae erat. Suspendisse potenti.".Replace(",", string.Empty).Replace(".", string.Empty);

            var random = new Random(1);
            string sample = all.Substring(_lipsumStart, random.Next(1, 30));
            _lipsumStart += 20;
            return sample;
        }

    }
}