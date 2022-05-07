// SPDX-License-Identifier: MIT
pragma solidity >=0.4.22 < 0.9.0;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";

contract StandardERC20 is ERC20 {
    constructor(uint initialSupply) ERC20("StandardERC20", "ERC20") {
        _mint(msg.sender, initialSupply);
    }
}
