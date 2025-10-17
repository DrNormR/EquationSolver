using System;
using System.Collections.Generic;
using System.Linq;
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

        private void MathButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string tag = button.Tag.ToString();
                int cursorPosition = EquationInput.SelectionStart;
                string currentText = EquationInput.Text;

                switch (tag)
                {
                    case "clear":
                        EquationInput.Text = "";
                        EquationInput.Focus();
                        break;

                    case "backspace":
                        if (cursorPosition > 0)
                        {
                            EquationInput.Text = currentText.Remove(cursorPosition - 1, 1);
                            EquationInput.SelectionStart = cursorPosition - 1;
                        }
                        EquationInput.Focus();
                        break;

                    case "^":
                        InsertTextAtCursor("^(");
                        break;

                    case "^2":
                        InsertTextAtCursor("^2");
                        break;

                    case "^(-1)":
                        InsertTextAtCursor("^(-1)");
                        break;

                    case "sqrt(":
                        InsertTextAtCursor("sqrt(");
                        break;

                    case "nroot(":
                        InsertTextAtCursor("nroot(");
                        break;

                    case "log(":
                        InsertTextAtCursor("log(");
                        break;

                    case "ln(":
                        InsertTextAtCursor("ln(");
                        break;

                    case "sin(":
                        InsertTextAtCursor("sin(");
                        break;

                    case "cos(":
                        InsertTextAtCursor("cos(");
                        break;

                    case "tan(":
                        InsertTextAtCursor("tan(");
                        break;

                    case "asin(":
                        InsertTextAtCursor("asin(");
                        break;

                    case "acos(":
                        InsertTextAtCursor("acos(");
                        break;

                    case "atan(":
                        InsertTextAtCursor("atan(");
                        break;

                    case "exp(":
                        InsertTextAtCursor("exp(");
                        break;

                    case "abs(":
                        InsertTextAtCursor("abs(");
                        break;

                    case "floor(":
                        InsertTextAtCursor("floor(");
                        break;

                    case "ceil(":
                        InsertTextAtCursor("ceil(");
                        break;

                    case "integrate(":
                        InsertTextAtCursor("integrate(");
                        break;

                    case "limit(":
                        InsertTextAtCursor("limit(");
                        break;

                    case "sum(":
                        InsertTextAtCursor("sum(");
                        break;

                    case "compose(":
                        InsertTextAtCursor("compose(");
                        break;

                    case "f(":
                        InsertTextAtCursor("f(");
                        break;

                    case "(":
                        InsertTextAtCursor("(");
                        break;

                    case "pi":
                        InsertTextAtCursor("pi");
                        break;

                    case "e":
                        InsertTextAtCursor("e");
                        break;

                    case "theta":
                        InsertTextAtCursor("theta");
                        break;

                    case "infinity":
                        InsertTextAtCursor("infinity");
                        break;

                    case "<=":
                        InsertTextAtCursor("<=");
                        break;

                    case ">=":
                        InsertTextAtCursor(">=");
                        break;

                    case "+-":
                        InsertTextAtCursor("+-");
                        break;

                    default:
                        InsertTextAtCursor(tag);
                        break;
                }
            }
        }

        private void InsertTextAtCursor(string text)
        {
            int cursorPosition = EquationInput.SelectionStart;
            string currentText = EquationInput.Text;
            
            EquationInput.Text = currentText.Insert(cursorPosition, text);
            EquationInput.SelectionStart = cursorPosition + text.Length;
            EquationInput.Focus();
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

                // Find the variable (assume it's 'x' for now)
                var variable = Var("x");

                // Create the equation object for solving
                var equation_obj = left.Equalizes(right);

                // Display steps with detailed explanations
                AddDetailedStep("Original Equation", $"{left} = {right}", 
                    "This is our starting equation. We need to solve for the variable x by isolating it on one side of the equation.", 
                    "#3498DB");

                // Show simplification steps
                var simplifiedLeft = left.Simplify();
                var simplifiedRight = right.Simplify();

                if (simplifiedLeft.ToString() != left.ToString() ||
                    simplifiedRight.ToString() != right.ToString())
                {
                    AddDetailedStep("Simplify", $"{simplifiedLeft} = {simplifiedRight}", 
                        "We simplify both sides of the equation by combining like terms and performing basic arithmetic operations.",
                        "#9B59B6");
                }

                // For linear equations, show detailed step-by-step solving
                if (IsLinearEquation(left, right, variable))
                {
                    SolveLinearStepByStepDetailed(simplifiedLeft, simplifiedRight, variable);
                }
                else
                {
                    // For other equations, show the solution process
                    AddDetailedStep("Apply solving algorithm",
                        $"Solve for {variable}", 
                        "For non-linear equations, we use advanced mathematical techniques such as factoring, quadratic formula, or numerical methods.",
                        "#E67E22");
                }

                // Solve the equation
                var solutions = equation_obj.Solve(variable);

                // Display solution(s) - Handle different solution types
                bool foundSolutions = false;

                // Check if it's a finite set of solutions
                if (solutions is Entity.Set solutionSet)
                {
                    if (solutionSet.DirectChildren.Any())
                    {
                        foundSolutions = true;
                        foreach (var solution in solutionSet.DirectChildren)
                        {
                            var simplified = solution.Simplify();
                            AddDetailedStep("Solution", $"{variable} = {simplified}", 
                                $"We have found that {variable} equals {simplified}. This is our final answer.",
                                "#27AE60", true);

                            // Verify the solution
                            try
                            {
                                var verification = left.Substitute(variable, simplified).Simplify();
                                var rightSimplified = right.Simplify();

                                AddDetailedStep("Verification",
                                    $"Left side: {verification.Evaled}\nRight side: {rightSimplified.Evaled}",
                                    $"Let's verify our answer by substituting {variable} = {simplified} back into the original equation. Both sides should be equal.",
                                    "#27AE60");
                            }
                            catch
                            {
                                // Skip verification if it fails
                            }
                        }
                    }
                }
                // Check if it's a single solution (not in a set)
                else if (solutions != null && !solutions.ToString().Contains("undefined") && !solutions.ToString().Contains("NaN"))
                {
                    foundSolutions = true;
                    var simplified = solutions.Simplify();
                    AddDetailedStep("Solution", $"{variable} = {simplified}", 
                        $"We have found that {variable} equals {simplified}. This is our final answer.",
                        "#27AE60", true);

                    // Verify the solution
                    try
                    {
                        var verification = left.Substitute(variable, simplified).Simplify();
                        var rightSimplified = right.Simplify();

                        AddDetailedStep("Verification",
                            $"Left side: {verification.Evaled}\nRight side: {rightSimplified.Evaled}",
                            $"Let's verify our answer by substituting {variable} = {simplified} back into the original equation. Both sides should be equal.",
                            "#27AE60");
                    }
                    catch
                    {
                        // Skip verification if it fails
                    }
                }

                if (!foundSolutions)
                {
                    // Try alternative solving approach
                    try
                    {
                        var eq = left - right;
                        var altSolutions = eq.SolveEquation(variable);
                        
                        if (altSolutions is Entity.Set altSet && altSet.DirectChildren.Any())
                        {
                            foundSolutions = true;
                            foreach (var solution in altSet.DirectChildren)
                            {
                                var simplified = solution.Simplify();
                                AddDetailedStep("Solution", $"{variable} = {simplified}", 
                                    $"Using alternative solving methods, we found that {variable} = {simplified}.",
                                    "#27AE60", true);
                            }
                        }
                        else if (altSolutions != null && !altSolutions.ToString().Contains("undefined"))
                        {
                            foundSolutions = true;
                            var simplified = altSolutions.Simplify();
                            AddDetailedStep("Solution", $"{variable} = {simplified}", 
                                $"Using alternative solving methods, we found that {variable} = {simplified}.",
                                "#27AE60", true);
                        }
                    }
                    catch
                    {
                        // Alternative approach failed
                    }
                }

                if (!foundSolutions)
                {
                    AddDetailedStep("Result", "No real solutions found", 
                        "After applying various mathematical techniques, we could not find any real number solutions for this equation. The equation may have complex solutions or no solutions at all.",
                        "#E74C3C", true);
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

        private bool IsLinearEquation(Entity left, Entity right, Entity variable)
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

        private void SolveLinearStepByStep(Entity left, Entity right, Entity variable)
        {
            // For linear equations like ax + b = c, show the steps
            try
            {
                // Example: 2*x + 5 = 13
                // Step 1: Subtract 5 from both sides: 2*x = 8
                // Step 2: Divide by 2: x = 4
                
                var leftSimp = left.Simplify();
                var rightSimp = right.Simplify();
                
                AddStep("Rearrange equation", 
                    $"Isolating {variable} step by step", "#E67E22");

                // Try to show intermediate steps for simple linear equations
                if (left.ToString().Contains("+") || left.ToString().Contains("-"))
                {
                    AddStep("Move constants", 
                        $"Move constant terms to the right side", "#E67E22");
                }
                
                if (left.ToString().Contains("*") && !left.ToString().Contains($"{variable}*{variable}"))
                {
                    AddStep("Divide by coefficient", 
                        $"Divide both sides by the coefficient of {variable}", "#E67E22");
                }
            }
            catch
            {
                // If step-by-step fails, continue with direct solution
                AddStep("Linear equation", 
                    $"Solving linear equation for {variable}", "#E67E22");
            }
        }

        private void SolveLinearStepByStepDetailed(Entity left, Entity right, Entity variable)
        {
            try
            {
                // Analyze the linear equation structure
                string leftStr = left.ToString();
                string rightStr = right.ToString();
                
                // Example: 2*x + 5 = 13
                if (leftStr.Contains("+") || leftStr.Contains("-"))
                {
                    // Step 1: Identify the parts
                    AddDetailedStep("Identify equation parts", 
                        $"Left side: {left}, Right side: {right}",
                        "In this linear equation, we can see terms with the variable and constant terms. Our goal is to isolate the variable term on one side.",
                        "#E67E22");
                    
                    // Step 2: Move constants
                    if (leftStr.Contains("+"))
                    {
                        AddDetailedStep("Subtract constant from both sides", 
                            "Move constant terms to the right side",
                            "When we have addition on the left side (like +5), we subtract the same amount from both sides to maintain equality. This isolates the variable term.",
                            "#E67E22");
                    }
                    else if (leftStr.Contains("-"))
                    {
                        AddDetailedStep("Add constant to both sides", 
                            "Move constant terms to the right side",
                            "When we have subtraction on the left side (like -5), we add the same amount to both sides to maintain equality. This isolates the variable term.",
                            "#E67E22");
                    }
                }
                
                // Step 3: Handle coefficient
                if (leftStr.Contains("*") && !leftStr.Contains($"{variable}*{variable}"))
                {
                    AddDetailedStep("Divide by coefficient", 
                        $"Divide both sides by the coefficient of {variable}",
                        $"The variable {variable} is multiplied by a coefficient. To isolate {variable}, we divide both sides by this coefficient. Division is the inverse operation of multiplication.",
                        "#E67E22");
                }
                else if (leftStr.Contains("/"))
                {
                    AddDetailedStep("Multiply by denominator", 
                        $"Multiply both sides to eliminate the fraction",
                        "When the variable is in a fraction, we multiply both sides by the denominator to eliminate the fraction and isolate the variable.",
                        "#E67E22");
                }
            }
            catch
            {
                // If detailed analysis fails, provide general explanation
                AddDetailedStep("Solve linear equation", 
                    $"Apply linear equation solving techniques for {variable}",
                    "For linear equations, we use inverse operations to isolate the variable: addition/subtraction for constants, and multiplication/division for coefficients.",
                    "#E67E22");
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

        private void AddDetailedStep(string title, string content, string explanation, string color, bool isHighlight = false)
        {
            var mainBorder = new Border
            {
                Background = isHighlight ?
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(color + "15")) :
                    Brushes.Transparent,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                BorderThickness = new Thickness(isHighlight ? 3 : 2, 0, 0, 0),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 15),
                CornerRadius = new CornerRadius(5)
            };

            var mainPanel = new StackPanel();

            // Title
            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = isHighlight ? 16 : 15,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                Margin = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Mathematical content
            var contentBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9ECEF")),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 10),
                CornerRadius = new CornerRadius(4)
            };

            var contentBlock = new TextBlock
            {
                Text = content,
                FontSize = isHighlight ? 20 : 18,
                FontFamily = new FontFamily("Cambria Math"),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            contentBorder.Child = contentBlock;

            // Explanation box (like the green box in your example)
            var explanationBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E8")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C3E6C3")),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(15, 12, 15, 12),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var explanationBlock = new TextBlock
            {
                Text = explanation,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C5234")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            explanationBorder.Child = explanationBlock;

            // Add all elements to the main panel
            mainPanel.Children.Add(titleBlock);
            mainPanel.Children.Add(contentBorder);
            mainPanel.Children.Add(explanationBorder);
            
            mainBorder.Child = mainPanel;
            StepsPanel.Children.Add(mainBorder);
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