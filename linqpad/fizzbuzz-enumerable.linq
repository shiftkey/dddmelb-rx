<Query Kind="Statements" />

Action<int> fizzBuzz = (n) => 
{
   var text = n.ToString() + " - ";
   if (n % 3 == 0) text += "fizz";
   if (n % 5 == 0) text += "buzz";
   Console.WriteLine(text);
};

var numbers = Enumerable.Range(1,20);
foreach(var number in numbers)
{
	fizzBuzz(number);
}
Console.WriteLine("done!");