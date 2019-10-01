import * as React from "react"
import { View, ViewStyle } from "react-native"
import { Spinner as BaseSpinner } from 'native-base';
import { palette } from "../../theme/palette";

export interface SpinnerProps {
  /**
   * Text which is looked up via i18n.
   */
  tx?: string

  /**
   * The text to display if not using `tx` or nested components.
   */
  text?: string

  /**
   * An optional style override useful for padding & margin.
   */
  style?: ViewStyle

  color?: string
}


/**
 * Stateless functional component for your needs
 *
 * Component description here for TypeScript tips.
 */
export function Spinner(props: SpinnerProps) {
  // grab the props
  const { tx, text, style, ...rest } = props
  const color = palette.red || props.color

  return (
    <View style={ style} {...rest}>
      <BaseSpinner color={ color }/>
    </View>
  )
}
