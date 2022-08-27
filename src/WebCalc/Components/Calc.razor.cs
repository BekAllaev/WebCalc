﻿using System.Diagnostics;
using WebCalc.Domain.BinaryOperation;

namespace WebCalc.Components
{
    public partial class Calc
    {
        public CalcDisplay? display;

        private async Task UpdateDisplayAsync(char value)
        {
            if (display is null) throw new NullReferenceException();

            if (value == Constants.MEMORY_ADD)
            {
                var memory = binaryOperationManager.GetMemoryAddResult(float.Parse(display!.Value));
                display.SetMemory(memory.ToString());
                return;
            }
            else if (value == Constants.MEMORY_CLEAR)
            {
                binaryOperationManager.ClearMemory();
                display.ClearMemory();
                return;
            }
            else if (
                TryToBackspaceResult(value) ||
                TryToBackspaceAndOperand2IsZero(value) ||
                TryToExceedMaxCountOfCharsOnDisplay(value) ||
                TryToNegateZero(value))
                return;
            else if (value == Constants.CLEAR)
                binaryOperationManager.BinaryOperation.Clear();
            else if (IsChainingCalculation(value))
            {
                binaryOperationManager.BinaryOperation.SetResult();
                binaryOperationManager.BinaryOperation.SetOperand(binaryOperationManager.BinaryOperation.Result);
                SetOperationType(value);

                var operand1 = binaryOperationManager.BinaryOperation.Operand1.ToString()!;
                var chars = new char[operand1.Count() + 1];
                operand1.ToArray().CopyTo(chars, 0);
                chars[operand1.Length] = value;

                display.Append(chars);
            }
            else if (value == '=')
            {
                binaryOperationManager.BinaryOperation.SetResult();
                await display.AppendAsync(value);
                display.Append(binaryOperationManager.BinaryOperation.Result.ToString()!.ToArray());

                return;
            }
            else
            {
                if (binaryOperationManager.BinaryOperation.OperationState is OperationState.ResultSetted && (value == '+' || value == '-' || value == '*' || value == '/'))
                {
                    display.Clear();
                    display.Append(binaryOperationManager.BinaryOperation.Result.ToString()!.ToArray());
                }

                if (binaryOperationManager.BinaryOperation.OperationState is OperationState.ResultSetted && (char.IsDigit(value) || value == Constants.FLOATING_POINT))
                {
                    display.Clear();
                    binaryOperationManager.BinaryOperation.Clear();
                }

                if (value == Constants.NEGATION_OPERATION_SIGN)
                {
                    binaryOperationManager.NegationOperation.SetOperand(float.Parse(display.Value));
                    binaryOperationManager.NegationOperation.SetResult();
                    display.Clear();
                    display.Append(binaryOperationManager.NegationOperation.Result.ToString()!.ToArray());
                }
            }

            await display.AppendAsync(value);
        }

        private void SetOperand(float operand)
        {
            binaryOperationManager.BinaryOperation.SetOperand(operand);
        }

        private void SetOperationType(OperationType operationType)
        {
            binaryOperationManager.BinaryOperation.SetOperationType(operationType);
        }

        private bool TryToBackspaceResult(char value)
            => binaryOperationManager.BinaryOperation.OperationState is OperationState.ResultSetted && value == Constants.BACKSPACE;

        private bool TryToExceedMaxCountOfCharsOnDisplay(char value)
            => display!.Value.Count() == display.MaxDisplayCharsCount && (char.IsDigit(value) || value == Constants.FLOATING_POINT);

        private bool TryToBackspaceAndOperand2IsZero(char value)
            => value == Constants.BACKSPACE && binaryOperationManager.BinaryOperation.Operand2 is 0;

        private bool TryToNegateZero(char value)
            => value == Constants.NEGATION_OPERATION_SIGN && display!.Value == "0";

        private bool IsChainingCalculation(char value) =>
            (value == '+' || value == '-' || value == '*' || value == '/') &&
            binaryOperationManager.BinaryOperation.Operand2 is not null;

        private void SetOperationType(char value)
        {
            switch (value)
            {
                case '+':
                    binaryOperationManager.BinaryOperation.SetOperationType(OperationType.Addition);
                    break;
                case '-':
                    binaryOperationManager.BinaryOperation.SetOperationType(OperationType.Subtraction);
                    break;
                case '*':
                    binaryOperationManager.BinaryOperation.SetOperationType(OperationType.Multiplication);
                    break;
                case '/':
                    binaryOperationManager.BinaryOperation.SetOperationType(OperationType.Division);
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }
}