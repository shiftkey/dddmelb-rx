<Query Kind="Statements">
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

Action<int> fizzBuzz = (n) => 
{
   var text = n.ToString() + " - ";
   if (n % 3 == 0) text += "fizz";
   if (n % 5 == 0) text += "buzz";
   Console.WriteLine(text);
};

var numbers = Observable.Interval(TimeSpan.FromSeconds(1));

numbers.Subscribe(
	(number) => fizzBuzz(number),
	() => Console.WriteLine("done!"));