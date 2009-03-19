using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Modular.Flair.WebPackage.Models;
using Spark.Modules;

namespace Modular.Flair.WebPackage.Services
{
    public interface ITriviaProvider : IService
    {
        Trivia GetRandomTrivia();
        Trivia GetTrivia(int triviaId);
    }

    public class TriviaProvider : ITriviaProvider
    {
        readonly Random _random = new Random();
        readonly IList<Trivia> _data;

        public TriviaProvider()
        {
            var serializer = new XmlSerializer(typeof(List<Trivia>));
            var type = typeof(Trivia);
            var stream = type.Assembly.GetManifestResourceStream(type, "Trivia.xml");
            _data = (IList<Trivia>)serializer.Deserialize(stream);
        }

        public Trivia GetRandomTrivia()
        {
            return _data[_random.Next(0, _data.Count - 1)];
        }

        public Trivia GetTrivia(int triviaId)
        {
            return _data.Single(t => t.Id == triviaId);
        }
    }
}
