import { useEffect, useRef } from "react";
import { useController } from "react-hook-form";
import { NumberFormatValues } from "react-number-format";

import currencyJS from "currency.js";

import { useFormContextSelector } from "../FormProvider";
import { useConfigurationValue } from "../configuration";
import { FormField, IFormFieldProps } from "../presentation";
import { CurrencyInput as PresentationCurrencyInput } from "../presentation/CurrencyInput";
import { IBasicFieldProps } from "./IBasicFieldProps";
import { useErrorMessage } from "./useErrorMessage";
import { useFormField } from "./useFormField";

interface IProps extends IBasicFieldProps, IFormFieldProps {
  currencyFieldName?: string;
  currency?: string;
  isDisabled?: boolean;
  onValueChange?(value: NumberFormatValues): void;
}

const CurrencyInput = ({
  register,
  currencyFieldName,
  currency = "PLN",
  defaultValue,
  backgroundColor,
  isDisabled,
  onValueChange,
  ...props
}: IProps) => {
  useFormField(props.name);

  const numberFormatValues = useRef<NumberFormatValues>({
    floatValue: undefined,
    formattedValue: "",
    value: "",
  });
  const control = useFormContextSelector((state) => state.control);
  const setValue = useFormContextSelector((state) => state.setValue);
  const autoValidation = useConfigurationValue("autoValidation");
  const error = useErrorMessage(props.name);
  const {
    field: { value, onChange, onBlur, ref },
  } = useController({
    name: props.name,
    control,
    defaultValue,
    rules: {
      required: {
        value: (autoValidation && props.isRequired) ?? false,
        message: "Pole jest wymagane.",
      },
      ...register,
    },
  });

  useEffect(() => {
    if (!currencyFieldName) {
      return;
    }
    if (!value) {
      setValue(props.name, null);
      return setValue(currencyFieldName, null);
    }
    setValue(currencyFieldName, currency);
  }, [currency, currencyFieldName, props.name, setValue, value]);

  return (
    <FormField
      label={props.children ?? props.label}
      isInvalid={!!error}
      errorMessage={error}
      {...props}
    >
      <PresentationCurrencyInput
        value={value ? currencyJS(value, { precision: 10 }).value : ""}
        onValueChange={(values) => {
          numberFormatValues.current = values;
        }}
        onChange={() => {
          const { floatValue, formattedValue, value } =
            numberFormatValues.current;

          if (onValueChange) {
            onValueChange({ floatValue, value, formattedValue });
          }
          if (floatValue === undefined) {
            return onChange(null);
          }
          onChange(value);
        }}
        onBlur={onBlur}
        getInputRef={ref}
        currency={currency}
        backgroundColor={backgroundColor}
        isDisabled={isDisabled}
      />
    </FormField>
  );
};

export { CurrencyInput };
