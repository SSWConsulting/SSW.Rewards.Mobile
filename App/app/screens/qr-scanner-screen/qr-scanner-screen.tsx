import React from "react"
import { observer } from "mobx-react"
import { ViewStyle } from "react-native"
import { Screen } from "../../components/screen"
// import { useStores } from "../../models/root-store"
import { color } from "../../theme"
import { NavigationScreenProps } from "react-navigation"
import QRCodeScanner from "react-native-qrcode-scanner"

export interface QrScannerScreenProps extends NavigationScreenProps<{}> {
}

const ROOT: ViewStyle = {
  backgroundColor: color.palette.black,
  height: '100%',
  alignItems: 'center'
}

export const QrScannerScreen: React.FunctionComponent<QrScannerScreenProps> = observer((props) => {
  // const { someStore } = useStores()
  const readSuccess = (e) => console.log(e.data);
  return (
    <Screen style={ROOT} preset="scroll">
        <QRCodeScanner
          onRead={readSuccess}
          reactivate={true}
          reactivateTimeout={3000}
        />
    </Screen>
  )
})
