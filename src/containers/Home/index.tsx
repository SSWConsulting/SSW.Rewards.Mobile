import React, { useContext, useEffect } from 'react'
import authentication from 'react-azure-adb2c';
import { AuthContext } from 'containers/Auth/B2CAuth';


const Home = (): JSX.Element => {

    const signOut = () => {
        console.log('auth: ', authentication.getAccessToken());
        authentication.signOut();
    }

    useEffect(() => {
        const token = authentication.getAccessToken();
        console.log(token);
        fetch('https://sswconsulting-dev.azurewebsites.net/api/leaderboard/get',{
            mode: 'no-cors',
            headers:{
            'Content-Type': 'application/json',
            Accept: 'application/json',
            'Authorization': `Bearer ${token}`,
            }
            
        }).then(e => {
            console.log(e);
        }).catch(e => {
            console.log(e);
        })
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
