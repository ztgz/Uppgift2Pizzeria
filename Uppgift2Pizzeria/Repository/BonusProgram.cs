using System;
using System.Collections.Generic;
using System.Linq;
using Uppgift2Pizzeria.Models;
using Uppgift2Pizzeria.ViewModels;

namespace Uppgift2Pizzeria.Repository
{
    public static class BonusProgram
    {
        //They recive free pizzas for every x points 
        private const int FreePizza = 100;
        //How many points they get per meal
        private const int PointsPerMeal = 10;
        //Discount percent on meals
        private const float DiscountPercent = 0.2f;

        public static int CalculatePoints(CheckoutViewModel vm)
        {
            return (vm.Meals.Count * PointsPerMeal);
        }

        public static int CalculateDeductionPoints(CheckoutViewModel vm)
        {
            int totalPoints = vm.BonusPointsAdded + vm.User.Poang;

            //There neeeds to be pizzas in order to recive free pizza
            int pizzasInOrder = vm.Meals.Count(m => m.MatrattTyp == 1);

            int pointsToDetuct = 0;

            if (totalPoints / FreePizza > 0 && pizzasInOrder > 0)
            {
                pointsToDetuct = Math.Min(totalPoints / FreePizza, pizzasInOrder);
            }

            return pointsToDetuct * FreePizza;
        }

        public static List<Matratt> GetFreePizzas(CheckoutViewModel vm)
        {
            List<Matratt> freeMeals = new List<Matratt>();

            //Calculate the number of pizzas that will be deducted
            int numbersOfFree = CalculateDeductionPoints(vm) / FreePizza;

            if (numbersOfFree > 0)
            {
                //Get all the pizzas in the order
                var allPizzas = vm.Meals.Where(m => m.MatrattTyp == 1).OrderBy(m => m.Pris).ToList();

                //Return the free pizzas
                freeMeals = allPizzas.GetRange(0, numbersOfFree);
            }

            return freeMeals;
        }

        public static int Discount(CheckoutViewModel vm)
        {
            return (int)((vm.Meals.Sum(p => p.Pris) - vm.FreeMeals.Sum(fm => fm.Pris)) * DiscountPercent);
        }
    }
}
