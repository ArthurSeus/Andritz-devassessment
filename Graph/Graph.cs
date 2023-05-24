using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;


namespace Graph
{
    public interface IGraph<T>
    {
        IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
    }

    public class Graph<T> : IGraph<T>
    {
        private List<string>[] adj;
        private List<string> differentLetters;

        public Graph(IEnumerable<ILink<T>> links)
        {
            differentLetters = DifferentVertices(links);
            int numberOfDifferentVertices = differentLetters.Count;

            //Console.WriteLine($"Number of different vertices: {numberOfDifferentVertices}");
            //Console.WriteLine($"Different letters: {string.Join(", ", differentLetters)}");

            adj = new List<string>[numberOfDifferentVertices];

            for (int i = 0; i < numberOfDifferentVertices; ++i)
            {
                adj[i] = new List<string>();
            }

            foreach (var link in links)
            {
                string from = link.Source.ToString();
                string to = link.Target.ToString();
                addEdge(from, to);
            }

        }

        private void addEdge(string from, string to)
        {
            // Add destination letter to the equivalent location of the letter list.
            int whereToPutEdge = differentLetters.IndexOf(from);
            adj[whereToPutEdge].Add(to);
        }

        private static List<string> DifferentVertices(IEnumerable<ILink<T>> links)
        {
            List<string> letters = new List<string>();

            foreach (var link in links)
            {
                string source = link.Source.ToString();
                string target = link.Target.ToString();

                if (!letters.Contains(source))
                {
                    letters.Add(source);
                }
                if (!letters.Contains(target))
                {
                    letters.Add(target);
                }
            }

            return letters;
        }


        public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
        {
            return (IObservable<IEnumerable<T>>)Observable.Create<IEnumerable<string>>(observer =>
            {
                var routes = new List<List<string>>();
                var currentRoute = new List<string>();
                var alreadyVisited = new HashSet<string>();

                void FindRoutes(string currentVertex)
                {
                    alreadyVisited.Add(currentVertex);
                    currentRoute.Add(currentVertex);

                    if (currentVertex.Equals(target.ToString()))
                    {
                        routes.Add(new List<string>(currentRoute));
                        observer.OnNext(new List<string>(currentRoute));
                    }
                    else
                    {
                        foreach (string neighbor in GetNeighbors(currentVertex))
                        {
                            if (!alreadyVisited.Contains(neighbor))
                            {
                                FindRoutes(neighbor);
                            }
                        }
                    }

                    alreadyVisited.Remove(currentVertex);
                    currentRoute.RemoveAt(currentRoute.Count - 1);
                }

                FindRoutes(source.ToString());

                observer.OnCompleted();

                return () => { }; // Dispose method
            });
        }

        private List<string> GetNeighbors(string vertex)
        {
            int whereToLook = differentLetters.IndexOf(vertex);
            return adj[whereToLook];
        }
    }
}