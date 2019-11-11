import React, { useContext, useEffect } from 'react'
import authentication from 'react-azure-adb2c';
import { AuthContext } from 'containers/Auth/B2CAuth';


const Home = (): JSX.Element => {

    const signOut = () => {
        console.log('auth: ', authentication.getAccessToken());
        authentication.signOut();
    }

    useEffect(() => {
        console.log('haha')
        
    }, [])

    const u = useContext(AuthContext);

    return (
        <div>
            {JSON.stringify(u)}
            <p>
                <button onClick={signOut}>Sign Out</button>
            </p>
        </div>
    )
}

export default Home
