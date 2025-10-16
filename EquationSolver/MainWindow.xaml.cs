using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AngouriMath;
using static AngouriMath.MathS;

namespace EquationSolver
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EquationInput.Text = "2*x + 5 = 13";
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StepsPanel.Children.Clear();

                string input = EquationInput.Text.Trim();
                if (string.IsNullOrWhiteSpace(input))
                {
                    ShowError("Please enter an equation.");
                    return;
                }

                // Parse the equation
                var equation = input;
                Entity expr = equation;

                // Show parsed equation
                ParsedEquationBorder.Visibility = Visibility.Visible;
                ParsedEquationText.Text = expr.Latexise(); // Or just use ToString() for simpler display

                // Try to solve the equation
                if (!input.Contains("="))
                {
                    ShowError("Please enter an equation with an '=' sign.");
                    return;
                }

                // Split into left and right side
                var parts = input.Split('=');
                if (parts.Length != 2)
                {
                    ShowError("Invalid equation format.");
                    return;
                }

                Entity left = parts[0].Trim();
                Entity right = parts[1].Trim();
                var eq = left.Equals(right);

                // Find the variable (assume it's 'x' for now)
                var variable = Variable("x");

                // Solve the equation
                var solutions = eq.Solve(variable);

                // Display steps
                AddStep("Original Equation", $"{left} = {right}", "#3498DB");

                // Show simplification steps
                var simplifiedLeft = left.Simplify();
                var simplifiedRight = right.Simplify();

                if (simplifiedLeft.ToString() != left.ToString() ||
                    simplifiedRight.ToString() != right.ToString())
                {
                    AddStep("Simplify", $"{simplifiedLeft} = {simplifiedRight}", "#9B59B6");
                }

                // For linear equations, show step-by-step solving
                if (IsLinearEquation(left, right, variable))
                {
                    SolveLinearStepByStep(simplifiedLeft, simplifiedRight, variable);
                }
                else
                {
                    // For other equations, just show the solution
                    AddStep("Apply solving algorithm",
                        $"Solve for {variable}", "#E67E22");
                }

                // Display solution(s)
                if (solutions.TryGetValue(out var solutionSet))
                {
                    foreach (var solution in solutionSet)
                    {
                        var simplified = solution.Simplify();
                        AddStep("Solution", $"{variable} = {simplified}", "#27AE60", true);

                        // Verify the solution
                        var verification = left.Substitute(variable, simplified).Simplify();
                        var rightSimplified = right.Simplify();

                        if (verification.Evaled().ToString() == rightSimplified.Evaled().ToString())
                        {
                            AddStep("Verification",
                                $"{verification} = {rightSimplified} ✓", "#27AE60");
                        }
                    }
                }
                else
                {
                    AddStep("Result", "No real solutions found", "#E74C3C", true);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}\n\nMake sure to use proper syntax:\n" +
                         "• Multiplication: 2*x\n" +
                         "• Powers: x^2\n" +
                         "• Square root: sqrt(x)\n" +
                         "• Fractions: (1/2)*x");
            }
        }

        private bool IsLinearEquation(Entity left, Entity right, Entity.Variable variable)
        {
            try
            {
                // Check if the equation is linear (highest power is 1)
                var combined = (left - right).Simplify();
                var vars = combined.Vars;

                if (!vars.Contains(variable))
                    return false;

                // Check degree
                foreach (var v in vars)
                {
                    if (v.ToString() == variable.ToString())
                    {
                        // Simple check: if it contains x^2 or higher, it's not linear
                        if (combined.ToString().Contains($"{variable}^") ||
                            combined.ToString().Contains($"{variable} ^"))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SolveLinearStepByStep(Entity left, Entity right, Entity.Variable variable)
        {
            // For linear equations like ax + b = c, show the steps
            try
            {
                // Move everything to left side
                var combined = (left - right).Simplify();
                AddStep("Move all terms to left side",
                    $"{combined} = 0", "#E67E22");

                // Try to isolate variable (this is simplified - real implementation would be more complex)
                AddStep("Isolate variable",
                    $"Rearrange to solve for {variable}", "#E67E22");
            }
            catch
            {
                // If step-by-step fails, continue with direct solution
            }
        }

        private void AddStep(string title, string content, string color, bool isHighlight = false)
        {
            var border = new Border
            {
                Background = isHighlight ?
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(color + "20")) :
                    Brushes.Transparent,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                BorderThickness = new Thickness(isHighlight ? 2 : 0, 0, 0, 0),
                Padding = new Thickness(isHighlight ? 15 : 10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var panel = new StackPanel();

            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var contentBlock = new TextBlock
            {
                Text = content,
                FontSize = isHighlight ? 18 : 16,
                FontFamily = new FontFamily("Cambria Math"),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                TextWrapping = TextWrapping.Wrap
            };

            panel.Children.Add(titleBlock);
            panel.Children.Add(contentBlock);
            border.Child = panel;

            StepsPanel.Children.Add(border);
        }

        private void ShowError(string message)
        {
            StepsPanel.Children.Clear();
            ParsedEquationBorder.Visibility = Visibility.Collapsed;

            var errorBlock = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 0)
            };

            StepsPanel.Children.Add(errorBlock);
        }
    }
}