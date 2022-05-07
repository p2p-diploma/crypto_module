// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;
import "./StandardERC20.sol";
contract ERC20TransferContract {
    StandardERC20 private token;
    mapping(address => mapping(address => uint)) private transactions;
    constructor(StandardERC20 token_) public {
        token = token_;
    }
    modifier is_sender(address recipient) {
        require(token.allowance(address(this), recipient) > 0, "Only the sender has permission to transfer");
        _;
    }
    //allow recipient to take from contract
    //allow contract to take from owner
    function block_sum(address recipient, uint amount) external {
        require(recipient != address(0), "Recipient's address does not exist");
        transactions[msg.sender][recipient] = amount;
        token.approve(recipient, amount);
    }

    function final_transfer(address recipient) external is_sender(recipient) {
        uint value = transactions[msg.sender][recipient];
        token.transferFrom(msg.sender, recipient, value);
        delete transactions[msg.sender][recipient];
    }

    function revert_transfer(address recipient) external is_sender(recipient) {
        uint value = transactions[msg.sender][recipient];
        token.decreaseAllowance(recipient, value);
        delete transactions[msg.sender][recipient];
    }
}