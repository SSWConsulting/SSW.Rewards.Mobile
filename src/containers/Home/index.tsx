import React from 'react'
import authentication from 'react-azure-adb2c';


const Home = () => {

    const signOut = () => {
        console.log('auth: ', authentication.getAccessToken());
        authentication.signOut();
    }

    return (
        <div>
            Home
            <p>
                <button onClick={signOut}>Sign Out</button>
            </p>
        </div>
    )
}

export default Home
